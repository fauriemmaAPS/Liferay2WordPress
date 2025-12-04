using System.Xml.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Liferay2WordPress.Models;

namespace Liferay2WordPress.Services;

public interface ILiferayArticleConverter
{
    ConvertedArticle ConvertToHtml(string contentXml, string defaultLocale);
}

public class LiferayArticleConverter : ILiferayArticleConverter
{
    // Pattern per rilevare HTML valido
    private static readonly Regex HtmlTagPattern = new Regex(
        @"<\s*([a-zA-Z][a-zA-Z0-9]*)\b[^>]*>.*?<\s*/\s*\1\s*>|<\s*[a-zA-Z][a-zA-Z0-9]*\b[^>]*/>|<\s*(img|br|hr|input|meta|link)\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled
    );

    public ConvertedArticle ConvertToHtml(string contentXml, string defaultLocale)
    {
        if (string.IsNullOrWhiteSpace(contentXml)) return new ConvertedArticle(string.Empty, new());

        try
        {
            var doc = XDocument.Parse(contentXml);
            var htmlParts = new List<string>();
            var urls = new List<string>();

            foreach (var dc in doc.Descendants("dynamic-content"))
            {
                var raw = (dc.Value ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                // Image field encoded as JSON
                if (raw.StartsWith("{") && raw.EndsWith("}"))
                {
                    try
                    {
                        using var jdoc = JsonDocument.Parse(raw);
                        var root = jdoc.RootElement;
                        var url = root.TryGetProperty("src", out var srcEl) ? srcEl.GetString() :
                                  root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            urls.Add(url!);
                            htmlParts.Add($"<p><img src=\"{System.Net.WebUtility.HtmlEncode(url)}\" alt=\"\" /></p>");
                            continue;
                        }
                    }
                    catch { }
                }

                // Verifica se il contenuto contiene HTML valido
                var hasHtmlTags = HtmlTagPattern.IsMatch(raw);

                if (hasHtmlTags)
                {
                    // Estrai tutti gli URL da src e href
                    ExtractUrlsFromHtml(raw, urls);
                    
                    // Il contenuto è già HTML, mantienilo così com'è
                    htmlParts.Add(raw);
                }
                else
                {
                    // Testo semplice -> wrappa in paragrafo
                    // Converti newlines multipli in paragrafi separati
                    var paragraphs = raw.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var para in paragraphs)
                    {
                        var trimmed = para.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            // Sostituisci singoli newline con <br />
                            var formatted = trimmed.Replace("\r\n", "<br />").Replace("\n", "<br />");
                            htmlParts.Add($"<p>{System.Net.WebUtility.HtmlEncode(formatted)}</p>");
                        }
                    }
                }
            }

            var html = string.Join("\n\n", htmlParts);
            return new ConvertedArticle(html, urls.Distinct().ToList());
        }
        catch
        {
            // Not XML or unexpected -> return raw as-is
            return new ConvertedArticle(contentXml, new());
        }
    }

    /// <summary>
    /// Estrae tutti gli URL da attributi src e href nel contenuto HTML
    /// </summary>
    private void ExtractUrlsFromHtml(string html, List<string> urls)
    {
        // Estrai URL da src (immagini, video, etc.)
        foreach (Match m in Regex.Matches(html, @"src\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.IgnoreCase))
        {
            var url = m.Groups["url"].Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                urls.Add(url);
            }
        }

        // Estrai URL da href (link a documenti)
        foreach (Match m in Regex.Matches(html, @"href\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.IgnoreCase))
        {
            var url = m.Groups["url"].Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                // Includi solo link che potrebbero essere media/documenti
                if (IsMediaUrl(url))
                {
                    urls.Add(url);
                }
            }
        }
    }

    /// <summary>
    /// Verifica se un URL punta probabilmente a un media/documento
    /// </summary>
    private bool IsMediaUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        
        // Link che contengono /documents/ (Liferay Document Library)
        if (url.Contains("/documents/", StringComparison.OrdinalIgnoreCase))
            return true;
        
        // Link che contengono /image/ (Liferay Image)
        if (url.Contains("/image/", StringComparison.OrdinalIgnoreCase))
            return true;
        
        // Pattern Liferay: /documents/NUMERO/NUMERO/FILENAME/UUID
        // Esempio: /documents/10181/40353/rilasciofontienergetiche.doc/db531154-f596-49e6-9cac-51b827ceb12f
        if (Regex.IsMatch(url, @"/documents/\d+/\d+/[^/]+/[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}", RegexOptions.IgnoreCase))
            return true;
        
        // Estensioni comuni di file media
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
}
