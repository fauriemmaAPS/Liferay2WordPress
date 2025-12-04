using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Liferay2WordPress.Data;

public interface ILiferayRepository
{
    IAsyncEnumerable<Liferay2WordPress.Models.LiferayArticle> GetArticlesAsync(long companyId, long groupId, string defaultLocale, bool onlyApproved, int batchSize, IEnumerable<long> excludeStructureIds, CancellationToken ct);
}

public class LiferayRepository : ILiferayRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayRepository> _logger;

    public LiferayRepository(string connectionString, ILogger<LiferayRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async IAsyncEnumerable<Liferay2WordPress.Models.LiferayArticle> GetArticlesAsync(long companyId, long groupId, string defaultLocale, bool onlyApproved, int batchSize, IEnumerable<long> excludeStructureIds, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var excludeList = excludeStructureIds?.ToList() ?? new List<long>();
        var excludeStr = excludeList.Select(x => x.ToString()).ToList();

        var approvalWhere = onlyApproved ? "AND a.status = 0" : string.Empty; // 0 = Approved
        var excludeWhere = excludeList.Count > 0 ? "AND (sl.structureId IS NULL OR sl.structureId NOT IN @excludeList) AND (a.structureId IS NULL OR a.structureId NOT IN @excludeStr)" : string.Empty;

        var sql = $@"
SELECT a.id_ AS Id,
       a.articleId AS ArticleId,
       a.version AS Version,
       a.title AS TitleRaw,
       a.content AS ContentXml,
       a.urlTitle AS UrlTitle,
       a.description AS DescriptionRaw,
       a.resourcePrimKey AS ResourcePrimKey,
       a.userId AS UserId,
       a.folderId AS FolderId,
       a.structureId AS StructureId,
       a.templateId AS TemplateId,
       a.createDate AS CreateDate,
       a.modifiedDate AS ModifiedDate
FROM journalarticle a
LEFT JOIN classname_ cn ON cn.value = 'com.liferay.portlet.journal.model.JournalArticle'
LEFT JOIN ddmstructurelink sl ON sl.classNameId = cn.classNameId AND sl.classPK = a.resourcePrimKey
JOIN (
  SELECT articleId, MAX(version) AS maxver
  FROM journalarticle a
  WHERE companyId = @companyId AND groupId = @groupId {approvalWhere}
  GROUP BY articleId
) latest ON latest.articleId = a.articleId AND latest.maxver = a.version
WHERE a.companyId = @companyId AND a.groupId = @groupId {approvalWhere} {excludeWhere}
ORDER BY a.modifiedDate ASC
LIMIT @limit OFFSET @offset";

        int offset = 0;
        while (!ct.IsCancellationRequested)
        {
            var rows = await conn.QueryAsync(sql, new { companyId, groupId, limit = batchSize, offset, excludeList, excludeStr });
            var list = rows.ToList();
            if (list.Count == 0) yield break;

            foreach (var r in list)
            {
                string title = TryGetLocalizedValue((string)r.TitleRaw, defaultLocale);
                string? desc = TryGetLocalizedValue((string?)r.DescriptionRaw ?? string.Empty, defaultLocale);
                var (cats, tags) = await GetAssetCategoriesAndTagsAsync(conn, (long)r.ResourcePrimKey, ct);

                yield return new Liferay2WordPress.Models.LiferayArticle
                {
                    Id = (long)r.Id,
                    ArticleId = (string)r.ArticleId,
                    Version = Convert.ToInt32(r.Version),
                    Title = title,
                    ContentXml = (string)r.ContentXml,
                    UrlTitle = (string?)r.UrlTitle,
                    Description = desc,
                    ResourcePrimKey = (long)r.ResourcePrimKey,
                    UserId = (long)r.UserId,
                    FolderId = r.FolderId is null ? null : (long)r.FolderId,
                    StructureId = (string?)r.StructureId,
                    TemplateId = (string?)r.TemplateId,
                    Categories = cats,
                    Tags = tags,
                    CreateDate = (DateTime)r.CreateDate,
                    ModifiedDate = (DateTime)r.ModifiedDate
                };
            }

            offset += list.Count;
        }
    }

    private static string TryGetLocalizedValue(string raw, string preferredLocale)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        if (raw.Contains("<root", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var x = XDocument.Parse(raw);
                var root = x.Root;
                if (root != null)
                {
                    var defaultLocale = root.Attribute("default-locale")?.Value;
                    var nodes = root.Elements().Where(e => string.Equals(e.Name.LocalName, "Title", StringComparison.OrdinalIgnoreCase) || string.Equals(e.Name.LocalName, "Description", StringComparison.OrdinalIgnoreCase));
                    var byPreferred = nodes.FirstOrDefault(e => string.Equals(e.Attribute("language-id")?.Value, preferredLocale, StringComparison.OrdinalIgnoreCase));
                    if (byPreferred != null) return byPreferred.Value.Trim();
                    var byDefault = nodes.FirstOrDefault(e => string.Equals(e.Attribute("language-id")?.Value, defaultLocale, StringComparison.OrdinalIgnoreCase));
                    if (byDefault != null) return byDefault.Value.Trim();
                    var any = nodes.FirstOrDefault();
                    if (any != null) return any.Value.Trim();
                }
            }
            catch { }
        }

        if (raw.TrimStart().StartsWith("{"))
        {
            try
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
                if (dict != null)
                {
                    if (dict.TryGetValue(preferredLocale, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
                    var any = dict.Values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    if (!string.IsNullOrWhiteSpace(any)) return any!;
                }
            }
            catch { }
        }

        return raw;
    }

    private static async Task<(List<string> categories, List<string> tags)> GetAssetCategoriesAndTagsAsync(MySqlConnection conn, long resourcePrimKey, CancellationToken ct)
    {
        var catsSql = @"SELECT c.name FROM assetcategory c
JOIN assetentries_assetcategories aec ON aec.categoryId = c.categoryId
JOIN assetentry e ON e.entryId = aec.entryId
WHERE e.classPK = @pk";

        var tagsSql = @"SELECT t.name FROM assettag t
JOIN assetentries_assettags aet ON aet.tagId = t.tagId
JOIN assetentry e ON e.entryId = aet.entryId
WHERE e.classPK = @pk";

        var cats = (await conn.QueryAsync<string>(catsSql, new { pk = resourcePrimKey })).ToList();
        var tags = (await conn.QueryAsync<string>(tagsSql, new { pk = resourcePrimKey })).ToList();
        return (cats, tags);
    }
}
