using System.Net.Mime;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Liferay2WordPress.Services;

public interface IMediaMigrator
{
    Task<(Dictionary<string, (int id, string url)> map, List<int> ids)> EnsureUploadedAsync(IEnumerable<string> urls, CancellationToken ct);
    Task<(string html, List<int> mediaIds)> RewriteLiferayDocumentLinksAsync(string html, string liferayBaseUrl, CancellationToken ct);
}

public class MediaMigrator : IMediaMigrator
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IWordPressClient _wp;
    private readonly ILogger<MediaMigrator> _logger;

    private readonly Dictionary<string, (int id, string url)> _cache = new(StringComparer.OrdinalIgnoreCase);

    public MediaMigrator(IHttpClientFactory httpFactory, IWordPressClient wp, ILogger<MediaMigrator> logger)
    {
        _httpFactory = httpFactory;
        _wp = wp;
        _logger = logger;
    }

    public async Task<(Dictionary<string, (int id, string url)> map, List<int> ids)> EnsureUploadedAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        var result = new Dictionary<string, (int id, string url)>(StringComparer.OrdinalIgnoreCase);
        var ids = new List<int>();

        var client = _httpFactory.CreateClient("media");
        foreach (var u in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            if (_cache.TryGetValue(u, out var cached))
            {
                result[u] = cached;
                ids.Add(cached.id);
                continue;
            }

            try
            {
                using var resp = await client.GetAsync(u, ct);
                resp.EnsureSuccessStatusCode();
                var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
                
                // Rileva MIME type dai byte se disponibile
                var mimeType = DetectMimeTypeFromBytes(bytes);
                
                // Se non rilevato, usa Content-Type header o guess dall'URL
                if (mimeType == "application/octet-stream")
                {
                    mimeType = resp.Content.Headers.ContentType?.MediaType ?? GuessMimeTypeFromUrl(u);
                }
                
                var fileName = GetFileNameFromUrl(u);
                
                // Aggiungi estensione se manca
                if (!System.IO.Path.HasExtension(fileName))
                {
                    var extension = GetExtensionFromMimeType(mimeType);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        fileName = $"{fileName}.{extension}";
                    }
                }

                _logger.LogInformation("Uploading {File} ({Size} bytes) as {MimeType}", fileName, bytes.Length, mimeType);

                var uploaded = await _wp.UploadMediaAsync(fileName, bytes, mimeType, ct);
                var mapped = (uploaded.Id, uploaded.SourceUrl);
                _cache[u] = mapped;
                result[u] = mapped;
                ids.Add(uploaded.Id);
                
                _logger.LogInformation("Successfully uploaded {File} -> {WordPressUrl} (ID: {MediaId})", fileName, uploaded.SourceUrl, uploaded.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload media from {Url}", u);
            }
        }

        return (result, ids);
    }

    public async Task<(string html, List<int> mediaIds)> RewriteLiferayDocumentLinksAsync(string html, string liferayBaseUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(html)) return (html, new());
        
        _logger.LogDebug("Starting URL rewrite. Base URL: {BaseUrl}", liferayBaseUrl ?? "(none)");
        
        // Dizionario: URL originale (come appare nell'HTML) -> URL normalizzato (per download)
        var urlMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Pattern migliorato per catturare URL da src e href
        var patterns = new[]
        {
            @"src\s*=\s*[""'](?<url>[^""']+)[""']",
            @"href\s*=\s*[""'](?<url>[^""']+)[""']"
        };

        foreach (var pattern in patterns)
        {
            foreach (Match m in Regex.Matches(html, pattern, RegexOptions.IgnoreCase))
            {
                var originalUrl = m.Groups["url"].Value;
                if (string.IsNullOrWhiteSpace(originalUrl)) continue;

                // Verifica se è un URL che dobbiamo riscrivere
                if (ShouldRewriteUrl(originalUrl, liferayBaseUrl))
                {
                    var normalizedUrl = NormalizeUrl(originalUrl, liferayBaseUrl);
                    if (!string.IsNullOrWhiteSpace(normalizedUrl))
                    {
                        // Salva la mappatura: URL originale -> URL normalizzato
                        urlMappings[originalUrl] = normalizedUrl;
                    }
                }
            }
        }

        if (urlMappings.Count == 0) return (html, new());

        _logger.LogInformation("Found {Count} media URLs to rewrite", urlMappings.Count);
        foreach (var kvp in urlMappings)
        {
            _logger.LogDebug("  Original: {Original} -> Normalized: {Normalized}", kvp.Key, kvp.Value);
        }

        // Upload usando gli URL normalizzati
        var normalizedUrls = urlMappings.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var upload = await EnsureUploadedAsync(normalizedUrls, ct);
        
        var updated = html;
        
        // Sostituisci gli URL originali (come appaiono nell'HTML) con gli URL WordPress
        foreach (var mapping in urlMappings)
        {
            var originalUrl = mapping.Key;
            var normalizedUrl = mapping.Value;
            
            // Trova l'URL WordPress corrispondente all'URL normalizzato
            if (upload.map.TryGetValue(normalizedUrl, out var wordpressMedia))
            {
                var wordpressUrl = wordpressMedia.url;
                
                // Sostituisci l'URL originale (che appare nell'HTML) con l'URL WordPress
                // Usa Regex.Escape per gestire caratteri speciali nell'URL
                updated = Regex.Replace(
                    updated,
                    Regex.Escape(originalUrl),
                    wordpressUrl,
                    RegexOptions.IgnoreCase
                );
                
                _logger.LogInformation("Rewritten {Original} -> {WordPress}", originalUrl, wordpressUrl);
            }
        }
        
        return (updated, upload.ids);
    }

    /// <summary>
    /// Determina se un URL dovrebbe essere riscritto (cioè se è un media/documento Liferay)
    /// </summary>
    private static bool ShouldRewriteUrl(string url, string liferayBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        
        // Non riscrivere URL esterni a meno che non siano del base URL Liferay
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(liferayBaseUrl) && 
                url.StartsWith(liferayBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return url.Contains("/documents/", StringComparison.OrdinalIgnoreCase) ||
                       url.Contains("/image/", StringComparison.OrdinalIgnoreCase);
            }
            return false; // URL esterno non Liferay
        }
        
        // URL relativi: controlla se contengono path di media Liferay
        return url.Contains("/documents/", StringComparison.OrdinalIgnoreCase) ||
               url.Contains("/image/", StringComparison.OrdinalIgnoreCase) ||
               HasMediaExtension(url);
    }

    /// <summary>
    /// Normalizza un URL (relativo -> assoluto se necessario)
    /// </summary>
    private static string NormalizeUrl(string url, string liferayBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        
        // Se è già assoluto, restituiscilo
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }
        
        // Se è relativo e abbiamo un base URL, combinali
        if (!string.IsNullOrWhiteSpace(liferayBaseUrl))
        {
            try
            {
                var baseUri = new Uri(liferayBaseUrl.TrimEnd('/') + "/");
                var combined = new Uri(baseUri, url.TrimStart('/'));
                return combined.ToString();
            }
            catch
            {
                return url; // Se fallisce, restituisci l'URL originale
            }
        }
        
        return url;
    }

    /// <summary>
    /// Verifica se un URL ha un'estensione di file media
    /// </summary>
    private static bool HasMediaExtension(string url)
    {
        var mediaExtensions = new[] { 
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico", ".tiff",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".zip", ".rar", ".7z", ".tar", ".gz",
            ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mpeg", ".mpg",
            ".mp3", ".wav", ".ogg", ".flac", ".aac",
            ".txt", ".csv", ".xml", ".json", ".odt", ".ods", ".odp"
        };
        
        var lowerUrl = url.ToLowerInvariant();
        return mediaExtensions.Any(ext => lowerUrl.Contains(ext));
    }

    /// <summary>
    /// Rileva il MIME type dai magic bytes del file
    /// </summary>
    private static string DetectMimeTypeFromBytes(byte[] bytes)
    {
        if (bytes.Length < 4) return "application/octet-stream";

        // PNG: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";

        // GIF: 47 49 46 38 (GIF8)
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            return "image/gif";

        // WEBP: 52 49 46 46 ... 57 45 42 50
        if (bytes.Length >= 12 && 
            bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return "image/webp";

        // PDF: 25 50 44 46 (%PDF)
        if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
            return "application/pdf";

        // ZIP / Office Open XML: 50 4B 03 04
        if (bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04)
        {
            // Controlla se è un documento Office Open XML
            if (bytes.Length > 100)
            {
                var str = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 500));
                if (str.Contains("word/")) return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                if (str.Contains("xl/")) return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (str.Contains("ppt/")) return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            }
            return "application/zip";
        }

        // DOC (MS Word 97-2003): D0 CF 11 E0
        if (bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0)
        {
            // Controlla signature più avanti per determinare tipo specifico
            if (bytes.Length > 512)
            {
                var str = System.Text.Encoding.ASCII.GetString(bytes.Skip(512).Take(100).ToArray());
                if (str.Contains("WordDocument")) return "application/msword";
                if (str.Contains("Workbook")) return "application/vnd.ms-excel";
                if (str.Contains("PowerPoint")) return "application/vnd.ms-powerpoint";
            }
            return "application/msword"; // Default to Word
        }

        // BMP: 42 4D (BM)
        if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            return "image/bmp";

        // TIFF: 49 49 2A 00 (little endian) or 4D 4D 00 2A (big endian)
        if ((bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) ||
            (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A))
            return "image/tiff";

        // SVG: <?xml or <svg
        if (bytes.Length > 5)
        {
            var start = System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(100, bytes.Length));
            if (start.TrimStart().StartsWith("<?xml") || start.TrimStart().StartsWith("<svg"))
                return "image/svg+xml";
        }

        // RAR: 52 61 72 21
        if (bytes.Length >= 4 && bytes[0] == 0x52 && bytes[1] == 0x61 && bytes[2] == 0x72 && bytes[3] == 0x21)
            return "application/x-rar-compressed";

        // 7Z: 37 7A BC AF 27 1C
        if (bytes.Length >= 6 && bytes[0] == 0x37 && bytes[1] == 0x7A && bytes[2] == 0xBC && 
            bytes[3] == 0xAF && bytes[4] == 0x27 && bytes[5] == 0x1C)
            return "application/x-7z-compressed";

        // MP4: vari signature, controlla ftyp
        if (bytes.Length >= 12)
        {
            var ftype = System.Text.Encoding.ASCII.GetString(bytes, 4, 4);
            if (ftype == "ftyp")
                return "video/mp4";
        }

        // MP3: FF FB or FF F3 or ID3
        if ((bytes[0] == 0xFF && (bytes[1] == 0xFB || bytes[1] == 0xF3)) ||
            (bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33))
            return "audio/mpeg";

        // Verifica se è un file di testo
        if (IsLikelyTextFile(bytes))
            return "text/plain";

        return "application/octet-stream";
    }

    /// <summary>
    /// Ottiene l'estensione file dal MIME type
    /// </summary>
    private static string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/gif" => "gif",
            "image/bmp" => "bmp",
            "image/webp" => "webp",
            "image/svg+xml" => "svg",
            "image/tiff" => "tiff",
            "image/x-icon" => "ico",
            
            "application/pdf" => "pdf",
            "application/zip" => "zip",
            "application/x-rar-compressed" => "rar",
            "application/x-7z-compressed" => "7z",
            
            "application/msword" => "doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
            "application/vnd.ms-excel" => "xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
            "application/vnd.ms-powerpoint" => "ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => "pptx",
            
            "application/vnd.oasis.opendocument.text" => "odt",
            "application/vnd.oasis.opendocument.spreadsheet" => "ods",
            "application/vnd.oasis.opendocument.presentation" => "odp",
            
            "text/plain" => "txt",
            "text/html" => "html",
            "text/csv" => "csv",
            "text/xml" => "xml",
            "application/xml" => "xml",
            "application/json" => "json",
            
            "video/mp4" => "mp4",
            "video/mpeg" => "mpeg",
            "video/quicktime" => "mov",
            "video/x-msvideo" => "avi",
            "video/x-ms-wmv" => "wmv",
            "video/webm" => "webm",
            
            "audio/mpeg" => "mp3",
            "audio/wav" => "wav",
            "audio/ogg" => "ogg",
            "audio/flac" => "flac",
            "audio/aac" => "aac",
            
            _ => "bin" // Fallback generico
        };
    }

    /// <summary>
    /// Controlla se il file sembra essere testo leggibile
    /// </summary>
    private static bool IsLikelyTextFile(byte[] bytes)
    {
        // Controlla i primi 1000 bytes per caratteri stampabili
        var sampleSize = Math.Min(1000, bytes.Length);
        var sample = bytes.Take(sampleSize).ToArray();
        
        // Conta caratteri non stampabili (esclusi tab, newline, carriage return)
        var nonPrintable = sample.Count(b => b < 32 && b != 9 && b != 10 && b != 13);
        
        // Se meno del 5% sono non stampabili, probabilmente è testo
        return nonPrintable < sampleSize * 0.05;
    }

    private static string Normalize(string url, string baseUrl)
    {
        return NormalizeUrl(url, baseUrl);
    }

    private static string GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var name = System.IO.Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(name)) name = "media";
            return name;
        }
        catch { return "media"; }
    }

    private static string GuessMimeTypeFromUrl(string url)
    {
        var ext = System.IO.Path.GetExtension(url).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => MediaTypeNames.Image.Jpeg,
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".ico" => "image/x-icon",
            
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            
            ".odt" => "application/vnd.oasis.opendocument.text",
            ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
            ".odp" => "application/vnd.oasis.opendocument.presentation",
            
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".csv" => "text/csv",
            ".xml" => "text/xml",
            ".json" => "application/json",
            
            ".mp4" => "video/mp4",
            ".mpeg" or ".mpg" => "video/mpeg",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".wmv" => "video/x-ms-wmv",
            ".webm" => "video/webm",
            
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            
            _ => "application/octet-stream"
        };
    }
}
