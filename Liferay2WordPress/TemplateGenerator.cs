using Liferay2WordPress.Data;
using Liferay2WordPress.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Liferay2WordPress;

public class TemplateGenerator
{
    private readonly ILiferayTemplateRepository _templatesRepo;
    private readonly ITemplateScriptConverter _scriptConverter;
    private readonly ILogger<TemplateGenerator> _logger;
    private readonly IConfiguration _config;

    public TemplateGenerator(
        ILiferayTemplateRepository templatesRepo, 
        ITemplateScriptConverter scriptConverter,
        ILogger<TemplateGenerator> logger, 
        IConfiguration config)
    {
        _templatesRepo = templatesRepo;
        _scriptConverter = scriptConverter;
        _logger = logger;
        _config = config;
    }

    public async Task GenerateTemplatesAsync(string outputDir, CancellationToken ct)
    {
        long groupId = _config.GetValue<long>("Liferay:GroupId");
        
        var templates = await _templatesRepo.GetAllTemplatesAsync(groupId, ct);
        
        if (templates.Count == 0)
        {
            _logger.LogWarning("No templates found for groupId {GroupId}", groupId);
            return;
        }

        Directory.CreateDirectory(outputDir);
        var templateMap = new Dictionary<string, string>();

        foreach (var template in templates)
        {
            var phpFileName = GeneratePhpTemplate(template, outputDir);
            templateMap[template.TemplateId] = phpFileName;
            _logger.LogInformation("Generated template: {TemplateId} -> {File}", template.TemplateId, phpFileName);
        }

        // Generate JSON config
        var jsonPath = Path.Combine(outputDir, "template-map.json");
        var json = JsonSerializer.Serialize(new { WordPress = new { TemplateMap = templateMap } }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, json, ct);
        
        _logger.LogInformation("Generated {Count} templates. Config saved to: {Path}", templates.Count, jsonPath);
        Console.WriteLine("\n=== Copy this to your appsettings.json WordPress section ===");
        Console.WriteLine(JsonSerializer.Serialize(new { TemplateMap = templateMap }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private string GeneratePhpTemplate(Models.LiferayTemplate template, string outputDir)
    {
        // template.Name contiene un XML cosi strutturato <?xml version='1.0' encoding='UTF-8'?><root available-locales="it_IT" default-locale="it_IT"><Name language-id="it_IT">PNRR_Scuole</Name></root>

        // Lettura del Xml per prendere il valore del Nodo Name con attributo language-id="it_IT"
        string templateName = template.Name;
        try
        {
            var doc = XDocument.Parse(template.Name);
            var nameNode = doc.Descendants("Name").FirstOrDefault(n => n.Attribute("language-id")?.Value == "it_IT");
            
            if (nameNode == null)
                nameNode = doc.Descendants("Name").FirstOrDefault(n => n.Attribute("language-id")?.Value == "en_US");

            if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.Value))
            {
                templateName = nameNode.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse template name XML, using raw value: {Name}", templateName);
        }

        var slug = SlugHelper.ToSlug(templateName).ToLower().Replace(" ", "-");
        var fileName = $"page-template-{template.TemplateId}-{slug}.php";
        var filePath = Path.Combine(outputDir, fileName);

        var phpContent = BuildPhpTemplate(template, templateName);
        File.WriteAllText(filePath, phpContent);

        return fileName;
    }

    private string BuildPhpTemplate(Models.LiferayTemplate template, string displayName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?php");
        sb.AppendLine("/**");
        sb.AppendLine($" * Template Name: {EscapePhpComment(displayName)}");
        sb.AppendLine($" * Template ID: {EscapePhpComment(template.TemplateId)}");
        sb.AppendLine(" * Generated from Liferay template");
        sb.AppendLine($" * Conversion: Advanced {GetLanguageName(template.Language)} to PHP");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine("get_header();");
        sb.AppendLine("?>");
        sb.AppendLine();
        
        sb.AppendLine("<div class=\"liferay-template-content\">");
        sb.AppendLine("    <?php");
        sb.AppendLine($"    // Original Liferay {GetLanguageName(template.Language)} template script:");
        sb.AppendLine("    /*");
        
        // Embed original Liferay script as comment
        foreach (var line in template.Script.Split('\n'))
        {
            sb.AppendLine($"    {line.TrimEnd()}");
        }
        
        sb.AppendLine("    */");
        sb.AppendLine("    ?>");
        sb.AppendLine();
        
        // Convert the template script to PHP
        var convertedPhp = _scriptConverter.ConvertToPhp(template.Script, template.Language);
        sb.AppendLine("    <!-- Converted template logic -->");
        sb.AppendLine(IndentPhpCode(convertedPhp, "    "));
        sb.AppendLine();
        
        sb.AppendLine("</div>");
        sb.AppendLine();
        sb.AppendLine("<?php");
        sb.AppendLine("get_footer();");
        
        return sb.ToString();
    }

    private string GetLanguageName(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "ftl" => "Freemarker",
            "vm" => "Velocity",
            _ => "Freemarker"
        };
    }

    private string IndentPhpCode(string code, string indent)
    {
        var lines = code.Split('\n');
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(indent + line.TrimEnd());
            else
                sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private string EscapePhpComment(string text)
    {
        return text.Replace("*/", "* /").Replace("<?", "<? ");
    }
}
