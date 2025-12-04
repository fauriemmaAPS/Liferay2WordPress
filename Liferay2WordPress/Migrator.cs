using Liferay2WordPress.Data;
using Liferay2WordPress.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Liferay2WordPress;

public class Migrator
{
    private readonly ILiferayRepository _repo;
    private readonly ILiferayUserRepository _users;
    private readonly ILiferayFolderRepository _foldersRepo;
    private readonly ILiferayTemplateRepository _templatesRepo;
    private readonly ILiferayArticleConverter _converter;
    private readonly IWordPressClient _wp;
    private readonly IMediaMigrator _media;
    private readonly IMigrationStateStore _stateStore;
    private readonly ILogger<Migrator> _logger;
    private readonly IConfiguration _config;

    public Migrator(ILiferayRepository repo, ILiferayUserRepository users, ILiferayFolderRepository foldersRepo, ILiferayTemplateRepository templatesRepo, ILiferayArticleConverter converter, IWordPressClient wp, IMediaMigrator media, IMigrationStateStore stateStore, ILogger<Migrator> logger, IConfiguration config)
    {
        _repo = repo;
        _users = users;
        _foldersRepo = foldersRepo;
        _templatesRepo = templatesRepo;
        _converter = converter;
        _wp = wp;
        _media = media;
        _stateStore = stateStore;
        _logger = logger;
        _config = config;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        long companyId = _config.GetValue<long>("Liferay:CompanyId");
        long groupId = _config.GetValue<long>("Liferay:GroupId");
        string defaultLocale = _config.GetValue<string>("Liferay:DefaultLocale") ?? "it_IT";
        int batchSize = _config.GetValue<int>("Migration:BatchSize");
        bool onlyApproved = _config.GetValue<bool>("Migration:OnlyApproved");
        string liferayBase = _config.GetValue<string>("Liferay:BaseUrl") ?? string.Empty;
        var excludeStructIds = (_config.GetSection("Migration:ExcludeStructureIds").Get<string[]>() ?? Array.Empty<string>())
            .Select(s => long.TryParse(s, out var id) ? id : (long?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
        string statePath = _config.GetValue<string>("Migration:StateFile") ?? "migration_state.json";

        var authorMap = _config.GetSection("WordPress:AuthorMap").Get<Dictionary<long, int>>() ?? new();
        var templateMap = _config.GetSection("WordPress:TemplateMap").Get<Dictionary<string, string>>() ?? new();

        // Load folders for parent chain mapping
        var folders = await _foldersRepo.GetFoldersAsync(groupId, ct);
        var folderById = folders.ToDictionary(f => f.FolderId, f => f);

        // Load state
        var state = await _stateStore.LoadAsync(statePath, ct);
        var createdPages = state.CreatedSlugToWpId;

        await foreach (var article in _repo.GetArticlesAsync(companyId, groupId, defaultLocale, onlyApproved, batchSize, excludeStructIds, ct))
        {
            if (state.CompletedArticleIds.Contains(article.ArticleId))
            {
                _logger.LogInformation("Skipping already migrated article {ArticleId}", article.ArticleId);
                continue;
            }

            var converted = _converter.ConvertToHtml(article.ContentXml, defaultLocale);

            var (htmlRewritten, dlMediaIds) = await _media.RewriteLiferayDocumentLinksAsync(converted.Html, liferayBase, ct);

            int? featured = null;
            if (converted.ImageUrls.Count > 0)
            {
                var uploadRes = await _media.EnsureUploadedAsync(converted.ImageUrls, ct);
                featured = uploadRes.ids.FirstOrDefault();
            }

            int[]? wpCats = null;
            if (article.Categories.Count > 0)
            {
                var ids = new List<int>();
                foreach (var c in article.Categories)
                {
                    var id = await _wp.EnsureTermAsync("categories", c, ct);
                    ids.Add(id);
                }
                wpCats = ids.ToArray();
            }

            int[]? wpTags = null;
            if (article.Tags.Count > 0)
            {
                var ids = new List<int>();
                foreach (var t in article.Tags)
                {
                    var id = await _wp.EnsureTermAsync("tags", t, ct);
                    ids.Add(id);
                }
                wpTags = ids.ToArray();
            }

            // Custom categories for pages (e.g., folder hierarchy -> taxonomy "page_category")
            var extraTax = new Dictionary<string, int[]>();
            var pageCats = await EnsurePageCategoriesAsync(article.FolderId, folderById, ct);
            if (pageCats.Length > 0)
            {
                extraTax["page_category"] = pageCats;
            }

            int? authorId = null;
            if (authorMap.TryGetValue(article.UserId, out var mapped))
            {
                authorId = mapped;
            }
            else
            {
                var (screenName, email, fullName) = await _users.GetUserAsync(article.UserId, ct);
                try
                {
                    authorId = await _wp.EnsureUserAsync(screenName, email, fullName, "author", ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to ensure user for {ScreenName} ({Email}), using null author", screenName, email);
                }
            }

            // Advanced slug rewriting: prefer UrlTitle, else from Title; ensure uniqueness
            var desiredSlug = !string.IsNullOrWhiteSpace(article.UrlTitle) ? article.UrlTitle! : SlugHelper.ToSlug(article.Title);
            var uniqueSlug = await EnsureUniqueSlugAsync(desiredSlug, ct);

            // Resolve parent by folder chain: parent slug is the closest ancestor with a created page
            int? parentId = await ResolveParentAsync(article.FolderId, folderById, createdPages, ct);

            string? wpTemplate = null;
            if (!string.IsNullOrWhiteSpace(article.TemplateId))
            {
                wpTemplate = ResolveTemplate(article.TemplateId, templateMap);
            }

            var post = new Models.WordPressPost
            {
                Title = article.Title,
                Content = htmlRewritten,
                Status = _config.GetValue<string>("WordPress:DefaultStatus") ?? "draft",
                Date = article.CreateDate,
                FeaturedMedia = featured,
                Slug = uniqueSlug,
                Author = authorId,
                Categories = wpCats,
                Tags = wpTags,
                Excerpt = article.Description,
                Parent = parentId,
                Template = wpTemplate
            };

            var created = await _wp.CreatePostAsync(post, extraTax.Count == 0 ? null : extraTax, ct);
            createdPages[uniqueSlug] = created.Id;
            state.CompletedArticleIds.Add(article.ArticleId);

            // Save state periodically
            await _stateStore.SaveAsync(statePath, state, ct);
        }

        await _stateStore.SaveAsync(statePath, state, ct);
    }

    private string? ResolveTemplate(string liferayTemplateId, Dictionary<string, string> templateMap)
    {
        // Try direct match by templateId
        if (templateMap.TryGetValue(liferayTemplateId, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
        {
            return mapped;
        }

        // Try uppercase/normalized key
        var normalized = liferayTemplateId.ToUpperInvariant().Replace("-", "_");
        if (templateMap.TryGetValue(normalized, out var mappedNorm) && !string.IsNullOrWhiteSpace(mappedNorm))
        {
            return mappedNorm;
        }

        // Fallback to DEFAULT
        if (templateMap.TryGetValue("DEFAULT", out var def))
        {
            return string.IsNullOrWhiteSpace(def) ? null : def;
        }

        return null;
    }

    private async Task<int?> ResolveParentAsync(long? folderId, Dictionary<long, Models.LiferayFolder> folderById, Dictionary<string, int> createdPages, CancellationToken ct)
    {
        if (folderId is null) return null;
        long current = folderId.Value;
        while (true)
        {
            if (!folderById.TryGetValue(current, out var folder)) return null;
            var slug = SlugHelper.ToSlug(folder.Name);
            if (createdPages.TryGetValue(slug, out var id)) return id;
            if (folder.ParentFolderId <= 0) return null;
            current = folder.ParentFolderId;
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(string desiredSlug, CancellationToken ct)
    {
        var slug = SlugHelper.ToSlug(desiredSlug);
        if (string.IsNullOrWhiteSpace(slug)) slug = "pagina";
        if (!await _wp.ExistsBySlugAsync(slug, ct)) return slug;
        int i = 2;
        while (await _wp.ExistsBySlugAsync($"{slug}-{i}", ct))
        {
            i++;
        }
        return $"{slug}-{i}";
    }

    private async Task<int[]> EnsurePageCategoriesAsync(long? folderId, Dictionary<long, Models.LiferayFolder> folderById, CancellationToken ct)
    {
        var ids = new List<int>();
        var names = new List<string>();
        long? current = folderId;
        var stack = new Stack<string>();
        while (current.HasValue)
        {
            if (!folderById.TryGetValue(current.Value, out var folder)) break;
            stack.Push(folder.Name);
            current = folder.ParentFolderId > 0 ? folder.ParentFolderId : null;
        }
        while (stack.Count > 0)
        {
            names.Add(stack.Pop());
        }
        foreach (var name in names)
        {
            var id = await _wp.EnsureTermAsync("page_category", name, ct);
            ids.Add(id);
        }
        return ids.ToArray();
    }
}
