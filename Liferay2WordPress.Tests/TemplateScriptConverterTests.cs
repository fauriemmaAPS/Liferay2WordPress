using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Liferay2WordPress.Services;

namespace Liferay2WordPress.Tests;

public class TemplateScriptConverterTests
{
    private readonly ITemplateScriptConverter _converter;

    public TemplateScriptConverterTests()
    {
        var mockLogger = new Mock<ILogger<TemplateScriptConverter>>();
        _converter = new TemplateScriptConverter(mockLogger.Object);
    }

    #region Freemarker Tests

    [Fact]
    public void ConvertFreemarker_SimpleVariableOutput_ConvertsCorrectly()
    {
        // Arrange
        var input = "${article.title}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("echo htmlspecialchars", result);
        Assert.Contains("$article", result);
    }

    [Fact]
    public void ConvertFreemarker_VariableWithDefault_ConvertsCorrectly()
    {
        // Arrange
        var input = "${title!\"Untitled\"}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("??", result); // Null coalescing
        Assert.Contains("Untitled", result);
    }

    [Fact]
    public void ConvertFreemarker_IfStatement_ConvertsCorrectly()
    {
        // Arrange
        var input = "<#if article.tags?has_content><div>Tags</div></#if>";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("if (!empty(", result);
        Assert.Contains("endif;", result);
    }

    [Fact]
    public void ConvertFreemarker_ListLoop_ConvertsCorrectly()
    {
        // Arrange
        var input = "<#list tags as tag>${tag}</#list>";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("foreach", result);
        Assert.Contains("$tags", result);
        Assert.Contains("$tag", result);
        Assert.Contains("endforeach", result);
    }

    [Fact]
    public void ConvertFreemarker_Assignment_ConvertsCorrectly()
    {
        // Arrange
        var input = "<#assign pageTitle = \"Welcome\">";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("$pageTitle", result);
        Assert.Contains("=", result);
        Assert.Contains("\"Welcome\"", result);
    }

    [Fact]
    public void ConvertFreemarker_Comment_ConvertsCorrectly()
    {
        // Arrange
        var input = "<#-- This is a comment -->";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("/*", result);
        Assert.Contains("This is a comment", result);
        Assert.Contains("*/", result);
    }

    [Fact]
    public void ConvertFreemarker_PropertyAccess_ConvertsCorrectly()
    {
        // Arrange
        var input = "${user.profile.name}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("$user['profile']['name']", result);
    }

    #endregion

    #region Velocity Tests

    [Fact]
    public void ConvertVelocity_SimpleVariableOutput_ConvertsCorrectly()
    {
        // Arrange
        var input = "$article.title";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("echo htmlspecialchars", result);
        Assert.Contains("$article", result);
    }

    [Fact]
    public void ConvertVelocity_SetDirective_ConvertsCorrectly()
    {
        // Arrange
        var input = "#set($pageTitle = \"Welcome\")";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("$pageTitle", result);
        Assert.Contains("=", result);
        Assert.Contains("\"Welcome\"", result);
    }

    [Fact]
    public void ConvertVelocity_IfStatement_ConvertsCorrectly()
    {
        // Arrange
        var input = "#if($user)Welcome#end";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("if (", result);
        Assert.Contains("$user", result);
        Assert.Contains("endif;", result);
    }

    [Fact]
    public void ConvertVelocity_ForeachLoop_ConvertsCorrectly()
    {
        // Arrange
        var input = "#foreach($item in $items)$item#end";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("foreach", result);
        Assert.Contains("$items", result);
        Assert.Contains("$item", result);
    }

    [Fact]
    public void ConvertVelocity_LineComment_ConvertsCorrectly()
    {
        // Arrange
        var input = "## This is a comment";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("//", result);
        Assert.Contains("This is a comment", result);
    }

    [Fact]
    public void ConvertVelocity_BlockComment_ConvertsCorrectly()
    {
        // Arrange
        var input = "#* Multi-line\ncomment *#";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("/*", result);
        Assert.Contains("Multi-line", result);
        Assert.Contains("*/", result);
    }

    #endregion

    #region Liferay Object Mapping Tests

    [Fact]
    public void ConvertToPhp_LiferayArticleTitle_MapsToWordPress()
    {
        // Arrange
        var input = "${article.title}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_title()", result);
    }

    [Fact]
    public void ConvertToPhp_LiferayUserFullName_MapsToWordPress()
    {
        // Arrange
        var input = "${user.fullName}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_author()", result);
    }

    [Fact]
    public void ConvertToPhp_LiferayArticleContent_MapsToWordPress()
    {
        // Arrange
        var input = "${article.content}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_content()", result);
    }

    [Fact]
    public void ConvertToPhp_LiferayArticleDate_MapsToWordPress()
    {
        // Arrange
        var input = "${article.createDate}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_date()", result);
    }

    [Fact]
    public void ConvertToPhp_LiferayCategories_MapsToWordPress()
    {
        // Arrange
        var input = "${article.categories}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_category()", result);
    }

    [Fact]
    public void ConvertToPhp_LiferayTags_MapsToWordPress()
    {
        // Arrange
        var input = "${article.tags}";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("get_the_tags()", result);
    }

    #endregion

    #region Complex Template Tests

    [Fact]
    public void ConvertToPhp_ComplexFreemarkerTemplate_ConvertsCorrectly()
    {
        // Arrange
        var input = @"
<#assign showAuthor = true>
<h1>${article.title}</h1>
<#if showAuthor>
    <p>By ${user.fullName}</p>
</#if>
<#list article.tags as tag>
    <span>${tag}</span>
</#list>";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("$showAuthor", result);
        Assert.Contains("get_the_title()", result);
        Assert.Contains("get_the_author()", result);
        Assert.Contains("foreach", result);
        Assert.Contains("if (", result);
        Assert.Contains("endif;", result);
    }

    [Fact]
    public void ConvertToPhp_ComplexVelocityTemplate_ConvertsCorrectly()
    {
        // Arrange
        var input = @"
#set($pageTitle = $article.title)
<h1>$pageTitle</h1>
#if($article.tags)
    #foreach($tag in $article.tags)
        <span>$tag</span>
    #end
#end";

        // Act
        var result = _converter.ConvertToPhp(input, "vm");

        // Assert
        Assert.Contains("$pageTitle", result);
        Assert.Contains("foreach", result);
        Assert.Contains("$tag", result);
        Assert.Contains("if (", result);
    }

    [Fact]
    public void ConvertToPhp_EmptyScript_ReturnsFallback()
    {
        // Arrange
        var input = "";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("the_content()", result);
    }

    [Fact]
    public void ConvertToPhp_NullScript_ReturnsFallback()
    {
        // Arrange
        string? input = null;

        // Act
        var result = _converter.ConvertToPhp(input!, "ftl");

        // Assert
        Assert.Contains("the_content()", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ConvertToPhp_MixedContent_PreservesHtml()
    {
        // Arrange
        var input = "<div class=\"wrapper\">${article.title}</div>";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        Assert.Contains("<div class=\"wrapper\">", result);
        Assert.Contains("</div>", result);
        Assert.Contains("get_the_title()", result);
    }

    [Fact]
    public void ConvertToPhp_NestedConditions_ConvertsCorrectly()
    {
        // Arrange
        var input = @"
<#if article.tags?has_content>
    <#if user??>
        <p>Tags by ${user.fullName}</p>
    </#if>
</#if>";

        // Act
        var result = _converter.ConvertToPhp(input, "ftl");

        // Assert
        var ifCount = System.Text.RegularExpressions.Regex.Matches(result, @"\bif\s*\(").Count;
        Assert.True(ifCount >= 2, "Should contain nested if statements");
    }

    #endregion
}
