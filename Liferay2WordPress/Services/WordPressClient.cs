using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Polly;

namespace Liferay2WordPress.Services;

public interface IWordPressClient
{
    Task<Models.WordPressPostResponse> CreatePostAsync(Models.WordPressPost post, Dictionary<string, int[]>? extraTaxonomies, CancellationToken ct);
    Task<Models.WordPressPostResponse> CreatePostAsync(Models.WordPressPost post, CancellationToken ct) => CreatePostAsync(post, null, ct);
    // New: explicit post type endpoint overrides
    Task<Models.WordPressPostResponse> CreatePostAsync(string postTypeEndpoint, Models.WordPressPost post, Dictionary<string, int[]>? extraTaxonomies, CancellationToken ct);
    Task<Models.WordPressPostResponse> CreatePostAsync(string postTypeEndpoint, Models.WordPressPost post, CancellationToken ct) => CreatePostAsync(postTypeEndpoint, post, null, ct);

    Task<Models.WordPressMediaResponse> UploadMediaAsync(string fileName, byte[] bytes, string contentType, CancellationToken ct);
    Task<int> EnsureTermAsync(string taxonomy, string name, CancellationToken ct);
    Task<int> EnsureUserAsync(string username, string email, string displayName, string role, CancellationToken ct);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct);
    // New: check slug existence for specific CPT endpoint
    Task<bool> ExistsBySlugAsync(string postTypeEndpoint, string slug, CancellationToken ct);
}

public class WordPressClient : IWordPressClient
{
    private readonly HttpClient _http;
    private readonly ILogger<WordPressClient> _logger;
    private readonly string _postType;
    private readonly Dictionary<string, string> _taxonomyRestBase = new(StringComparer.OrdinalIgnoreCase);

    public WordPressClient(HttpClient http, ILogger<WordPressClient> logger, Uri baseUri, string username, string appPassword, string postType = "posts")
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = baseUri;
        _postType = string.IsNullOrWhiteSpace(postType) ? "posts" : postType;

        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{appPassword}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Models.WordPressPostResponse> CreatePostAsync(Models.WordPressPost post, Dictionary<string, int[]>? extraTaxonomies, CancellationToken ct)
    {
        var node = JsonSerializer.SerializeToNode(post) as JsonObject ?? new JsonObject();
        if (extraTaxonomies != null)
        {
            foreach (var kvp in extraTaxonomies)
            {
                node[kvp.Key] = JsonSerializer.SerializeToNode(kvp.Value);
            }
        }
        var payload = node.ToJsonString();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var retry = Policy.Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));

        var response = await retry.ExecuteAsync(() => _http.PostAsync($"/wp-json/wp/v2/{_postType}", content, ct));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var res = JsonSerializer.Deserialize<Models.WordPressPostResponse>(json) ?? throw new InvalidOperationException("Invalid WP response");
        _logger.LogInformation("Created WP {Type} {Id} {Link}", _postType.TrimEnd('s'), res.Id, res.Link);
        return res;
    }

    // New: Create post for specific CPT endpoint
    public async Task<Models.WordPressPostResponse> CreatePostAsync(string postTypeEndpoint, Models.WordPressPost post, Dictionary<string, int[]>? extraTaxonomies, CancellationToken ct)
    {
        var node = JsonSerializer.SerializeToNode(post) as JsonObject ?? new JsonObject();
        if (extraTaxonomies != null)
        {
            foreach (var kvp in extraTaxonomies)
            {
                node[kvp.Key] = JsonSerializer.SerializeToNode(kvp.Value);
            }
        }
        var payload = node.ToJsonString();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var retry = Policy.Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));

        var target = string.IsNullOrWhiteSpace(postTypeEndpoint) ? _postType : postTypeEndpoint;
        var response = await retry.ExecuteAsync(() => _http.PostAsync($"/wp-json/wp/v2/{target}", content, ct));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var res = JsonSerializer.Deserialize<Models.WordPressPostResponse>(json) ?? throw new InvalidOperationException("Invalid WP response");
        _logger.LogInformation("Created WP {Type} {Id} {Link}", target.TrimEnd('s'), res.Id, res.Link);
        return res;
    }

    public async Task<Models.WordPressMediaResponse> UploadMediaAsync(string fileName, byte[] bytes, string contentType, CancellationToken ct)
    {
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };

        var retry = Policy.Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, i => 
            {
                _logger.LogWarning("Retry {Attempt} after {Delay}s", i, Math.Pow(2, i));
                return TimeSpan.FromSeconds(Math.Pow(2, i));
            });

    HttpResponseMessage response;
    try
    {
        response = await retry.ExecuteAsync(() => _http.PostAsync("/wp-json/wp/v2/media", content, ct));
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to upload media {FileName} after retries", fileName);
        throw;
    }

    // ⭐ LOGGING DETTAGLIATO
    if (!response.IsSuccessStatusCode)
    {
        var errorBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Media Upload Failed {StatusCode}: {Error} | File: {FileName} Size: {Size}", 
            response.StatusCode, errorBody, fileName, bytes.Length);
        response.EnsureSuccessStatusCode(); // Questo genererà l'eccezione con dettagli
    }
    
    var json = await response.Content.ReadAsStringAsync(ct);
    var res = JsonSerializer.Deserialize<Models.WordPressMediaResponse>(json) ?? throw new InvalidOperationException("Invalid media response");
    _logger.LogInformation("Uploaded media {Id} {Url}", res.Id, res.SourceUrl);
    return res;
}

    public async Task<int> EnsureTermAsync(string taxonomy, string name, CancellationToken ct)
    {
        var baseName = await GetTaxonomyRestBaseAsync(taxonomy, ct) ?? taxonomy;

        var encoded = Uri.EscapeDataString(name);
        var listResp = await _http.GetAsync($"/wp-json/wp/v2/{baseName}?search={encoded}&per_page=1", ct);
        if (listResp.IsSuccessStatusCode)
        {
            var listJson = await listResp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(listJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                var id = doc.RootElement[0].GetProperty("id").GetInt32();
                return id;
            }
        }
        else if (listResp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Taxonomy endpoint not found: {Endpoint}. Ensure taxonomy '{Tax}' is registered with show_in_rest=true.", $"/wp-json/wp/v2/{baseName}", taxonomy);
        }

        var payload = JsonSerializer.Serialize(new { name });
        var createResp = await _http.PostAsync($"/wp-json/wp/v2/{baseName}", new StringContent(payload, Encoding.UTF8, "application/json"), ct);
        createResp.EnsureSuccessStatusCode();
        var createJson = await createResp.Content.ReadAsStringAsync(ct);
        using var created = JsonDocument.Parse(createJson);
        return created.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<string?> GetTaxonomyRestBaseAsync(string taxonomy, CancellationToken ct)
    {
        if (_taxonomyRestBase.TryGetValue(taxonomy, out var cached)) return cached;

        var resp = await _http.GetAsync("/wp-json/wp/v2/taxonomies", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var slug = prop.Name; // taxonomy slug key
                var restBase = prop.Value.TryGetProperty("rest_base", out var rb) ? rb.GetString() : null;
                if (!string.IsNullOrWhiteSpace(slug) && !string.IsNullOrWhiteSpace(restBase))
                {
                    _taxonomyRestBase[slug] = restBase!;
                }
            }
        }

        return _taxonomyRestBase.TryGetValue(taxonomy, out var val) ? val : null;
    }

    public async Task<int> EnsureUserAsync(string username, string email, string displayName, string role, CancellationToken ct)
    {
        var q = Uri.EscapeDataString(email);
        var getResp = await _http.GetAsync($"/wp-json/wp/v2/users?search={q}&context=edit", ct);
        if (getResp.IsSuccessStatusCode)
        {
            var json = await getResp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var user in doc.RootElement.EnumerateArray())
                {
                    if (user.TryGetProperty("email", out var emailProp) && string.Equals(emailProp.GetString(), email, StringComparison.OrdinalIgnoreCase))
                    {
                        return user.GetProperty("id").GetInt32();
                    }
                }
            }
        }

        var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var payload = new
        {
            username = username,
            email = email,
            name = displayName,
            roles = new[] { string.IsNullOrWhiteSpace(role) ? "author" : role },
            password = password
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var createResp = await _http.PostAsync("/wp-json/wp/v2/users", content, ct);
        createResp.EnsureSuccessStatusCode();
        var createdJson = await createResp.Content.ReadAsStringAsync(ct);
        using var createdDoc = JsonDocument.Parse(createdJson);
        return createdDoc.RootElement.GetProperty("id").GetInt32();
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/wp-json/wp/v2/{_postType}?slug={Uri.EscapeDataString(slug)}&per_page=1", ct);
        if (!resp.IsSuccessStatusCode) return false;
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0;
    }

    // New: Exists by slug for specific CPT endpoint
    public async Task<bool> ExistsBySlugAsync(string postTypeEndpoint, string slug, CancellationToken ct)
    {
        var target = string.IsNullOrWhiteSpace(postTypeEndpoint) ? _postType : postTypeEndpoint;
        var resp = await _http.GetAsync($"/wp-json/wp/v2/{target}?slug={Uri.EscapeDataString(slug)}&per_page=1", ct);
        if (!resp.IsSuccessStatusCode) return false;
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0;
    }
}
