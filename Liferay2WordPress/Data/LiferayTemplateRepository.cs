using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Liferay2WordPress.Data;

public interface ILiferayTemplateRepository
{
    Task<Models.LiferayTemplate?> GetTemplateAsync(string templateId, CancellationToken ct);
    Task<List<Models.LiferayTemplate>> GetAllTemplatesAsync(long groupId, CancellationToken ct);
}

public class LiferayTemplateRepository : ILiferayTemplateRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayTemplateRepository> _logger;

    public LiferayTemplateRepository(string connectionString, ILogger<LiferayTemplateRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<Models.LiferayTemplate?> GetTemplateAsync(string templateId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        var sql = @"SELECT templateId, name, script, language FROM ddmtemplate WHERE templateId = @templateId OR templateKey = @templateId LIMIT 1";
        var row = await conn.QuerySingleOrDefaultAsync(sql, new { templateId });
        if (row == null) return null;
        
        var language = DetermineTemplateLanguage((string?)row.language, (string)row.script);
        return new Models.LiferayTemplate("" + row.templateId, (string)row.name, (string)row.script)
        {
            Language = language
        };
    }

    public async Task<List<Models.LiferayTemplate>> GetAllTemplatesAsync(long groupId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        var sql = @"SELECT templateId, name, script, language FROM ddmtemplate WHERE groupId = @groupId";
        var rows = await conn.QueryAsync(sql, new { groupId });
        var list = new List<Models.LiferayTemplate>();
        foreach (var r in rows)
        {
            var language = DetermineTemplateLanguage((string?)r.language, (string)r.script);
            var template = new Models.LiferayTemplate("" + r.templateId, (string)r.name, (string)r.script)
            {
                Language = language
            };
            list.Add(template);
        }
        return list;
    }

    private static string DetermineTemplateLanguage(string? languageColumn, string script)
    {
        // First check the language column from database
        if (!string.IsNullOrWhiteSpace(languageColumn))
        {
            var lang = languageColumn.ToLowerInvariant();
            if (lang.Contains("ftl") || lang.Contains("freemarker"))
                return "ftl";
            if (lang.Contains("vm") || lang.Contains("velocity"))
                return "vm";
        }

        // Fallback: Detect from script content
        if (script.Contains("<#") || script.Contains("${"))
            return "ftl";
        if (script.Contains("#set") || script.Contains("#if") || script.Contains("#foreach"))
            return "vm";

        // Default to Freemarker
        return "ftl";
    }
}
