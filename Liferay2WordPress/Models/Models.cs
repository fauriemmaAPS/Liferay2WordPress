using System.Text.Json;
using System.Text.Json.Serialization;

namespace Liferay2WordPress.Models;

public record LiferayArticle
{
    public long Id { get; init; }
    public string ArticleId { get; init; } = string.Empty;
    public int Version { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ContentXml { get; init; } = string.Empty;
    public string? UrlTitle { get; init; }
    public string? Description { get; init; }
    public long ResourcePrimKey { get; init; }
    public long UserId { get; init; }
    public long? FolderId { get; init; }
    public string? StructureId { get; init; }
    public string? TemplateId { get; init; }
    public List<string> Categories { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public DateTime CreateDate { get; init; }
    public DateTime ModifiedDate { get; init; }
}

public record LiferayFolder(long FolderId, string Name, long ParentFolderId);

public record LiferayTemplate(string TemplateId, string Name, string Script)
{
    public string Language { get; init; } = "ftl"; // ftl (Freemarker), vm (Velocity), or other
}

public record WordPressPost
{
    [JsonPropertyName("title")] public required string Title { get; init; }
    [JsonPropertyName("content")] public required string Content { get; init; }
    [JsonPropertyName("status")] public string Status { get; init; } = "publish";
    [JsonPropertyName("date")] public DateTime? Date { get; init; }
    [JsonPropertyName("featured_media")] public int? FeaturedMedia { get; init; }
    [JsonPropertyName("slug")] public string? Slug { get; init; }
    [JsonPropertyName("author")] public int? Author { get; init; }
    [JsonPropertyName("categories")] public int[]? Categories { get; init; }
    [JsonPropertyName("tags")] public int[]? Tags { get; init; }
    [JsonPropertyName("excerpt")] public string? Excerpt { get; init; }
    [JsonPropertyName("parent")] public int? Parent { get; init; }
    [JsonPropertyName("template")] public string? Template { get; init; }
}

public record WordPressPostResponse
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("link")] public string Link { get; init; } = string.Empty;
}

public record WordPressMediaResponse
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("source_url")] public string SourceUrl { get; init; } = string.Empty;
}

public record ConvertedArticle(string Html, List<string> ImageUrls);
