using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace Liferay2WordPress.Services;

/// <summary>
/// Converts Liferay Freemarker/Velocity template scripts to WordPress PHP
/// </summary>
public interface ITemplateScriptConverter
{
    string ConvertToPhp(string script, string templateLanguage = "ftl");
}

public class TemplateScriptConverter : ITemplateScriptConverter
{
    private readonly ILogger<TemplateScriptConverter> _logger;

    public TemplateScriptConverter(ILogger<TemplateScriptConverter> logger)
    {
        _logger = logger;
    }

    public string ConvertToPhp(string script, string templateLanguage = "ftl")
    {
        if (string.IsNullOrWhiteSpace(script))
            return "<?php the_content(); ?>";

        var isFreemarker = templateLanguage.Equals("ftl", StringComparison.OrdinalIgnoreCase) || 
                          templateLanguage.Equals("freemarker", StringComparison.OrdinalIgnoreCase);
        var isVelocity = templateLanguage.Equals("vm", StringComparison.OrdinalIgnoreCase) || 
                        templateLanguage.Equals("velocity", StringComparison.OrdinalIgnoreCase);

        try
        {
            if (isFreemarker)
                return ConvertFreemarkerToPhp(script);
            else if (isVelocity)
                return ConvertVelocityToPhp(script);
            else
                return ConvertFreemarkerToPhp(script); // Default to Freemarker
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert template script, returning fallback");
            return GenerateFallbackPhp(script);
        }
    }

    private string ConvertFreemarkerToPhp(string script)
    {
        var php = script;

        // Convert Freemarker comments to PHP comments
        php = Regex.Replace(php, @"<#--\s*(.*?)\s*-->", "<?php /* $1 */ ?>", RegexOptions.Singleline);

        // Convert variable assignments: <#assign varName = value>
        php = Regex.Replace(php, @"<#assign\s+(\w+)\s*=\s*([^>]+)>", match =>
        {
            var varName = match.Groups[1].Value;
            var value = ConvertFreemarkerExpression(match.Groups[2].Value.Trim());
            return $"<?php ${varName} = {value}; ?>";
        });

        // Convert variable output: ${variable} or ${variable!"default"}
        php = Regex.Replace(php, @"\$\{([^}]+)\}", match =>
        {
            var expr = match.Groups[1].Value;
            return ConvertFreemarkerVariableOutput(expr);
        });

        // Convert if statements: <#if condition>
        php = Regex.Replace(php, @"<#if\s+([^>]+)>", match =>
        {
            var condition = ConvertFreemarkerCondition(match.Groups[1].Value.Trim());
            return $"<?php if ({condition}) : ?>";
        });

        // Convert elseif: <#elseif condition>
        php = Regex.Replace(php, @"<#elseif\s+([^>]+)>", match =>
        {
            var condition = ConvertFreemarkerCondition(match.Groups[1].Value.Trim());
            return $"<?php elseif ({condition}) : ?>";
        });

        // Convert else: <#else>
        php = Regex.Replace(php, @"<#else>", "<?php else : ?>");

        // Convert endif: </#if>
        php = Regex.Replace(php, @"</#if>", "<?php endif; ?>");

        // Convert list/foreach: <#list items as item>
        php = Regex.Replace(php, @"<#list\s+(\w+)\s+as\s+(\w+)>", match =>
        {
            var items = match.Groups[1].Value;
            var item = match.Groups[2].Value;
            return $"<?php if (!empty(${items})) : foreach (${items} as ${item}) : ?>";
        });

        // Convert endlist: </#list>
        php = Regex.Replace(php, @"</#list>", "<?php endforeach; endif; ?>");

        // Convert Liferay-specific objects to WordPress equivalents
        php = ConvertLiferayObjectsToWordPress(php);

        // Convert method calls: object.method() or object.property
        php = Regex.Replace(php, @"\$\{(\w+)\.(\w+)(?:\(\))?\}", match =>
        {
            var obj = match.Groups[1].Value;
            var method = match.Groups[2].Value;
            return $"<?php echo ${obj}['{method}']; ?>";
        });

        return php;
    }

    private string ConvertVelocityToPhp(string script)
    {
        var php = script;

        // Convert Velocity comments to PHP comments
        php = Regex.Replace(php, @"##\s*(.*?)$", "<?php // $1 ?>", RegexOptions.Multiline);
        php = Regex.Replace(php, @"#\*\s*(.*?)\s*\*#", "<?php /* $1 */ ?>", RegexOptions.Singleline);

        // Convert variable assignments: #set($varName = value)
        php = Regex.Replace(php, @"#set\s*\(\s*\$(\w+)\s*=\s*([^)]+)\)", match =>
        {
            var varName = match.Groups[1].Value;
            var value = ConvertVelocityExpression(match.Groups[2].Value.Trim());
            return $"<?php ${varName} = {value}; ?>";
        });

        // Convert variable output: $variable or ${variable}
        php = Regex.Replace(php, @"\$!?\{?([a-zA-Z_][\w\.]*)\}?", match =>
        {
            var expr = match.Groups[1].Value;
            if (expr.Contains("."))
                return ConvertVelocityPropertyAccess(expr);
            return $"<?php echo htmlspecialchars(${expr} ?? '', ENT_QUOTES); ?>";
        });

        // Convert if statements: #if($condition)
        php = Regex.Replace(php, @"#if\s*\(\s*([^)]+)\)", match =>
        {
            var condition = ConvertVelocityCondition(match.Groups[1].Value.Trim());
            return $"<?php if ({condition}) : ?>";
        });

        // Convert elseif: #elseif($condition)
        php = Regex.Replace(php, @"#elseif\s*\(\s*([^)]+)\)", match =>
        {
            var condition = ConvertVelocityCondition(match.Groups[1].Value.Trim());
            return $"<?php elseif ({condition}) : ?>";
        });

        // Convert else: #else
        php = Regex.Replace(php, @"#else", "<?php else : ?>");

        // Convert end: #end
        php = Regex.Replace(php, @"#end", "<?php endif; ?>");

        // Convert foreach: #foreach($item in $items)
        php = Regex.Replace(php, @"#foreach\s*\(\s*\$(\w+)\s+in\s+\$(\w+)\)", match =>
        {
            var item = match.Groups[1].Value;
            var items = match.Groups[2].Value;
            return $"<?php if (!empty(${items})) : foreach (${items} as ${item}) : ?>";
        });

        // Convert Liferay-specific objects to WordPress equivalents
        php = ConvertLiferayObjectsToWordPress(php);

        return php;
    }

    private string ConvertFreemarkerVariableOutput(string expr)
    {
        // Handle default values: variable!"default"
        var match = Regex.Match(expr, @"^([^!]+)!""([^""]+)""$");
        if (match.Success)
        {
            var varName = match.Groups[1].Value.Trim();
            var defaultValue = match.Groups[2].Value;
            var phpVar = ConvertFreemarkerExpression(varName);
            return $"<?php echo htmlspecialchars({phpVar} ?? '{defaultValue}', ENT_QUOTES); ?>";
        }

        // Handle property access: object.property
        if (expr.Contains("."))
        {
            var parts = expr.Split('.');
            var phpExpr = "$" + parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                phpExpr += $"['{parts[i]}']";
            }
            return $"<?php echo htmlspecialchars({phpExpr} ?? '', ENT_QUOTES); ?>";
        }

        // Simple variable
        return $"<?php echo htmlspecialchars(${expr} ?? '', ENT_QUOTES); ?>";
    }

    private string ConvertFreemarkerCondition(string condition)
    {
        // Convert has_content check
        condition = Regex.Replace(condition, @"(\w+)\?has_content", "!empty($$1)");
        
        // Convert ?? operator to isset
        condition = Regex.Replace(condition, @"(\w+)\?\?", "isset($$1)");
        
        // Convert string equality
        condition = Regex.Replace(condition, @"(\w+)\s*==\s*""([^""]+)""", "$$1 === '$2'");
        
        // Convert negation
        condition = condition.Replace("!", "!");
        
        return condition;
    }

    private string ConvertVelocityCondition(string condition)
    {
        // Remove $ prefix and convert to PHP variable
        condition = Regex.Replace(condition, @"\$(\w+)", "$$1");
        
        // Convert null checks
        condition = condition.Replace("== null", "=== null");
        condition = condition.Replace("!= null", "!== null");
        
        return condition;
    }

    private string ConvertFreemarkerExpression(string expr)
    {
        // Convert string literals
        if (expr.StartsWith("\"") && expr.EndsWith("\""))
            return expr;
        
        // Convert numbers
        if (int.TryParse(expr, out _) || double.TryParse(expr, out _))
            return expr;
        
        // Convert boolean
        if (expr == "true" || expr == "false")
            return expr;
        
        // Convert property access
        if (expr.Contains("."))
        {
            var parts = expr.Split('.');
            var phpExpr = "$" + parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                phpExpr += $"['{parts[i]}']";
            }
            return phpExpr;
        }
        
        // Variable reference
        return "$" + expr;
    }

    private string ConvertVelocityExpression(string expr)
    {
        // Convert string literals
        if (expr.StartsWith("\"") && expr.EndsWith("\""))
            return expr;
        
        // Convert variable references
        expr = Regex.Replace(expr, @"\$(\w+)", "$$1");
        
        return expr;
    }

    private string ConvertVelocityPropertyAccess(string expr)
    {
        var parts = expr.Split('.');
        var phpExpr = "$" + parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            phpExpr += $"['{parts[i]}']";
        }
        return $"<?php echo htmlspecialchars({phpExpr} ?? '', ENT_QUOTES); ?>";
    }

    private string ConvertLiferayObjectsToWordPress(string php)
    {
        // Map common Liferay objects to WordPress equivalents
        var mappings = new Dictionary<string, string>
        {
            // Journal article content
            { "$journalArticle", "$post" },
            { "$article", "$post" },
            
            // User objects
            { "$user.fullName", "get_the_author()" },
            { "$user.firstName", "get_the_author_meta('first_name')" },
            { "$user.lastName", "get_the_author_meta('last_name')" },
            { "$user.emailAddress", "get_the_author_meta('user_email')" },
            
            // Date/time
            { "$article.createDate", "get_the_date()" },
            { "$article.modifiedDate", "get_the_modified_date()" },
            
            // Content fields
            { "$article.title", "get_the_title()" },
            { "$article.description", "get_the_excerpt()" },
            { "$article.content", "get_the_content()" },
            
            // URLs
            { "$article.url", "get_permalink()" },
            { "$article.urlTitle", "get_post_field('post_name')" },
            
            // Categories and tags
            { "$article.categories", "get_the_category()" },
            { "$article.tags", "get_the_tags()" },
            
            // Asset related
            { "$assetEntry", "$post" },
            { "$assetRenderer", "$post" },
        };

        foreach (var mapping in mappings)
        {
            php = php.Replace(mapping.Key, mapping.Value);
        }

        // Convert Liferay service calls to WordPress functions
        php = Regex.Replace(php, @"\$serviceLocator\.findService\([^)]+\)", "/* WordPress service call */");
        php = Regex.Replace(php, @"\$themeDisplay\.getSiteGroupId\(\)", "get_current_blog_id()");
        php = Regex.Replace(php, @"\$themeDisplay\.getUser\(\)", "wp_get_current_user()");
        php = Regex.Replace(php, @"\$themeDisplay\.getURLCurrent\(\)", "$_SERVER['REQUEST_URI']");

        // Convert asset publisher specific objects
        php = php.Replace("$entry.getTitle($locale)", "get_the_title()");
        php = php.Replace("$entry.getDescription($locale)", "get_the_excerpt()");
        php = php.Replace("$entry.getSummary($locale)", "get_the_excerpt()");

        return php;
    }

    private string GenerateFallbackPhp(string originalScript)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?php");
        sb.AppendLine("/**");
        sb.AppendLine(" * Original Liferay template script:");
        sb.AppendLine(" * (Could not be automatically converted)");
        sb.AppendLine(" */");
        sb.AppendLine("/*");
        foreach (var line in originalScript.Split('\n'))
        {
            sb.AppendLine(line.TrimEnd());
        }
        sb.AppendLine("*/");
        sb.AppendLine();
        sb.AppendLine("// Fallback: Display WordPress content");
        sb.AppendLine("the_content();");
        sb.AppendLine("?>");
        return sb.ToString();
    }
}
