using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.Json;

namespace Liferay2WordPress.Data;

/// <summary>
/// Repository per leggere le DDMStructure (Data Definition Models) da Liferay
/// Queste strutture definiscono i content types con i loro campi custom
/// </summary>
public interface ILiferayStructureRepository
{
    Task<List<DDMStructure>> GetAllStructuresAsync(long groupId, CancellationToken ct);
    Task<DDMStructure?> GetStructureAsync(long structureId, CancellationToken ct);
}

public class LiferayStructureRepository : ILiferayStructureRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayStructureRepository> _logger;

    public LiferayStructureRepository(string connectionString, ILogger<LiferayStructureRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<List<DDMStructure>> GetAllStructuresAsync(long groupId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        
        // Controlla se esiste la tabella ddmstructureversion (Liferay 7.x+)
        var hasVersionTable = await CheckIfVersionTableExistsAsync(conn, ct);
        
        _logger.LogInformation("Detected Liferay version: {Version}", hasVersionTable ? "7.x+" : "6.x (using ddmstructure.definition directly)");
        
        string sql;
        if (hasVersionTable)
        {
            // Liferay 7.x+ - usa ddmstructureversion
            sql = @"
                SELECT 
                    s.structureId,
                    s.structureKey,
                    s.classNameId,
                    s.name,
                    s.description,
                    sv.definition
                FROM ddmstructure s
                INNER JOIN ddmstructureversion sv ON s.structureId = sv.structureId
                WHERE s.groupId = @groupId
                AND sv.version = (
                    SELECT MAX(version) 
                    FROM ddmstructureversion 
                    WHERE structureId = s.structureId
                )
                ORDER BY s.name";
        }
        else
        {
            // Liferay 6.x - definition è direttamente in ddmstructure
            // In Liferay 6.2 la colonna si chiama 'xsd' non 'definition'
            sql = @"
                SELECT 
                    structureId,
                    structureKey,
                    classNameId,
                    name,
                    description,
                    xsd as definition
                FROM ddmstructure
                WHERE groupId = @groupId
                ORDER BY name";
        }

        var rows = await conn.QueryAsync(sql, new { groupId });
        var structures = new List<DDMStructure>();

        foreach (var row in rows)
        {
            try
            {
                var structure = new DDMStructure
                {
                    StructureId = (long)row.structureId,
                    StructureKey = (string)row.structureKey,
                    ClassNameId = (long)row.classNameId,
                    Name = ExtractNameFromXml((string)row.name),
                    Description = row.description?.ToString() ?? string.Empty,
                    Definition = (string)row.definition
                };

                // Parse i campi dalla definizione JSON/XML
                structure.Fields = ParseFieldsFromDefinition(structure.Definition);
                
                _logger.LogDebug("Loaded structure {Name} with {FieldCount} fields", 
                    structure.Name, structure.Fields.Count);
                
                structures.Add(structure);
            }
            catch (Exception ex)
            {
                string key = row.structureKey?.ToString() ?? "unknown";
                _logger.LogWarning("Failed to parse structure {StructureKey}: {Error}", key, ex.Message);
            }
        }

        return structures;
    }

    public async Task<DDMStructure?> GetStructureAsync(long structureId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        
        // Controlla se esiste la tabella ddmstructureversion (Liferay 7.x+)
        var hasVersionTable = await CheckIfVersionTableExistsAsync(conn, ct);
        
        string sql;
        if (hasVersionTable)
        {
            // Liferay 7.x+ - usa ddmstructureversion
            sql = @"
                SELECT 
                    s.structureId,
                    s.structureKey,
                    s.classNameId,
                    s.name,
                    s.description,
                    sv.definition
                FROM ddmstructure s
                INNER JOIN ddmstructureversion sv ON s.structureId = sv.structureId
                WHERE s.structureId = @structureId
                AND sv.version = (
                    SELECT MAX(version) 
                    FROM ddmstructureversion 
                    WHERE structureId = s.structureId
                )
                LIMIT 1";
        }
        else
        {
            // Liferay 6.x - definition è nella colonna 'xsd'
            sql = @"
                SELECT 
                    structureId,
                    structureKey,
                    classNameId,
                    name,
                    description,
                    xsd as definition
                FROM ddmstructure
                WHERE structureId = @structureId
                LIMIT 1";
        }

        var row = await conn.QuerySingleOrDefaultAsync(sql, new { structureId });
        if (row == null) return null;

        var structure = new DDMStructure
        {
            StructureId = (long)row.structureId,
            StructureKey = (string)row.structureKey,
            ClassNameId = (long)row.classNameId,
            Name = ExtractNameFromXml((string)row.name),
            Description = row.description?.ToString() ?? string.Empty,
            Definition = (string)row.definition
        };

        structure.Fields = ParseFieldsFromDefinition(structure.Definition);
        return structure;
    }

    /// <summary>
    /// Verifica se esiste la tabella ddmstructureversion (Liferay 7.x+)
    /// </summary>
    private async Task<bool> CheckIfVersionTableExistsAsync(MySqlConnection conn, CancellationToken ct)
    {
        try
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = DATABASE() 
                AND table_name = 'ddmstructureversion'";
            
            var count = await conn.ExecuteScalarAsync<int>(sql);
            var exists = count > 0;
            
            _logger.LogDebug("DDMStructureVersion table exists: {Exists} (Liferay {Version})", 
                exists, exists ? "7.x+" : "6.x");
            
            return exists;
        }
        catch
        {
            // In caso di errore, assumiamo Liferay 6.x
            _logger.LogDebug("Failed to check version table, assuming Liferay 6.x");
            return false;
        }
    }

    /// <summary>
    /// Parse la definizione JSON/XML della struttura per estrarre i campi
    /// </summary>
    private List<DDMField> ParseFieldsFromDefinition(string definition)
    {
        var fields = new List<DDMField>();

        try
        {
            // Liferay 7.x usa JSON
            if (definition.TrimStart().StartsWith("{"))
            {
                fields = ParseJsonDefinition(definition);
            }
            // Liferay 6.x usa XML
            else if (definition.TrimStart().StartsWith("<"))
            {
                fields = ParseXmlDefinition(definition);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse field definition");
        }

        return fields;
    }

    /// <summary>
    /// Parse definizione JSON (Liferay 7.x+)
    /// </summary>
    private List<DDMField> ParseJsonDefinition(string json)
    {
        var fields = new List<DDMField>();
        
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Cerca array "fields" o "availableLanguageIds"
        if (root.TryGetProperty("fields", out var fieldsArray))
        {
            foreach (var fieldEl in fieldsArray.EnumerateArray())
            {
                var field = new DDMField
                {
                    Name = fieldEl.GetProperty("name").GetString() ?? string.Empty,
                    Label = GetLocalizedValue(fieldEl, "label"),
                    Type = fieldEl.GetProperty("type").GetString() ?? "text",
                    DataType = fieldEl.TryGetProperty("dataType", out var dt) ? dt.GetString() : "string",
                    Required = fieldEl.TryGetProperty("required", out var req) && req.GetBoolean(),
                    Repeatable = fieldEl.TryGetProperty("repeatable", out var rep) && rep.GetBoolean(),
                    Tip = GetLocalizedValue(fieldEl, "tip")
                };

                // Options per select/radio/checkbox
                if (fieldEl.TryGetProperty("options", out var opts))
                {
                    field.Options = ParseOptions(opts);
                }

                fields.Add(field);
            }
        }

        return fields;
    }

    /// <summary>
    /// Parse definizione XML (Liferay 6.x)
    /// </summary>
    private List<DDMField> ParseXmlDefinition(string xml)
    {
        var fields = new List<DDMField>();
        
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xml);
            
            foreach (var fieldEl in doc.Descendants("dynamic-element"))
            {
                var field = new DDMField
                {
                    Name = fieldEl.Attribute("name")?.Value ?? string.Empty,
                    Type = fieldEl.Attribute("type")?.Value ?? "text",
                    DataType = fieldEl.Attribute("dataType")?.Value ?? "string",
                    Required = fieldEl.Attribute("required")?.Value == "true",
                    Repeatable = fieldEl.Attribute("repeatable")?.Value == "true"
                };

                // Label e tip da metadata
                var metaDataEl = fieldEl.Element("meta-data");
                if (metaDataEl != null)
                {
                    var labelEntry = metaDataEl.Elements("entry")
                        .FirstOrDefault(e => e.Attribute("name")?.Value == "label");
                    if (labelEntry != null)
                    {
                        field.Label = labelEntry.Value ?? string.Empty;
                    }
                }

                fields.Add(field);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to parse XML definition: {Error}", ex.Message);
        }

        return fields;
    }

    private static string GetLocalizedValue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return string.Empty;

        // Se è un oggetto localizzato, prendi il primo valore disponibile
        if (prop.ValueKind == JsonValueKind.Object)
        {
            foreach (var localeProp in prop.EnumerateObject())
            {
                return localeProp.Value.GetString() ?? string.Empty;
            }
        }

        return prop.GetString() ?? string.Empty;
    }

    private List<FieldOption> ParseOptions(JsonElement optionsElement)
    {
        var options = new List<FieldOption>();

        if (optionsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var opt in optionsElement.EnumerateArray())
            {
                options.Add(new FieldOption
                {
                    Value = opt.GetProperty("value").GetString() ?? string.Empty,
                    Label = GetLocalizedValue(opt, "label")
                });
            }
        }

        return options;
    }

    private static string ExtractNameFromXml(string xml)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xml);
            var nameElement = doc.Root?.Element("Name");
            if (nameElement != null)
            {
                // se nel nameElement.Value è presente un apice questo va corredato di escape
                var name = nameElement.Value;
                if (name != null)
                    return name.Replace("'", "\\'");
            }
        }
        catch
        {
            // Ignora errori di parsing
        }
        return "Unnamed_Structure";
    }
}

/// <summary>
/// Rappresenta una DDMStructure di Liferay (content type)
/// </summary>
public class DDMStructure
{
    public long StructureId { get; set; }
    public string StructureKey { get; set; } = string.Empty;
    public long ClassNameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public List<DDMField> Fields { get; set; } = new();
    
    /// <summary>
    /// Ottiene il tipo di struttura basato sul classNameId
    /// </summary>
    public string GetStructureTypeName()
    {
        return ClassNameId switch
        {
            10109 => "CMN - Contenuti", // JournalArticle - Strutture
            10098 => "CMN - Liste", // DDLRecordSet - Liste
            10091 => "CMN - Metadati", // DLFileEntry - Documenti
            _ => "CMN - Altri Tipi" // Tipo sconosciuto
        };
    }
    
    /// <summary>
    /// Ottiene lo slug del menu parent basato sul classNameId
    /// </summary>
    public string GetParentMenuSlug()
    {
        return ClassNameId switch
        {
            10109 => "cmn_web_content",
            10198 => "cmn_data_lists",
            10091 => "cmn_metadata",
            _ => "cmn_other"
        };
    }
    
    /// <summary>
    /// Ottiene l'icona dashicon del menu parent basato sul classNameId
    /// </summary>
    public string GetParentMenuIcon()
    {
        return ClassNameId switch
        {
            10109 => "dashicons-media-document",
            10198 => "dashicons-list-view",
            10091 => "dashicons-media-default",
            _ => "dashicons-admin-generic"
        };
    }
}

/// <summary>
/// Rappresenta un campo custom di una struttura
/// </summary>
public class DDMField
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // text, textarea, select, radio, checkbox, date, number, etc.
    public string DataType { get; set; } = "string"; // string, number, date, boolean, etc.
    public bool Required { get; set; }
    public bool Repeatable { get; set; }
    public string Tip { get; set; } = string.Empty;
    public List<FieldOption> Options { get; set; } = new();
}

/// <summary>
/// Opzione per campi select/radio/checkbox
/// </summary>
public class FieldOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
