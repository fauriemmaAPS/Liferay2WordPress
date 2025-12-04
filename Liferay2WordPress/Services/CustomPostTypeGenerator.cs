using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Liferay2WordPress.Data;

namespace Liferay2WordPress.Services;

/// <summary>
/// Modalità di generazione per Custom Post Types e Taxonomies
/// </summary>
public enum GenerationMode
{
    /// <summary>
    /// Genera file PHP separati per post types e taxonomies
    /// </summary>
    PHP,

    /// <summary>
    /// Genera file JSON per ACF PRO (Post Types, Taxonomies e Fields gestiti da ACF)
    /// </summary>
    ACF_JSON
}

/// <summary>
/// Servizio per generare custom post types WordPress con ACF da strutture Liferay
/// </summary>
public interface ICustomPostTypeGenerator
{
    Task GenerateFromLiferayStructuresAsync(long groupId, CancellationToken ct);
    Task<string> GenerateACFJsonAsync(DDMStructure structure, CancellationToken ct);
    Task<string> GeneratePostTypeCodeAsync(DDMStructure structure, int menuPosition, CancellationToken ct);
}

public class CustomPostTypeGenerator : ICustomPostTypeGenerator
{
    private readonly ILiferayStructureRepository _structureRepo;
    private readonly ILogger<CustomPostTypeGenerator> _logger;
    private readonly string _outputPath;
    private readonly GenerationMode _mode;

    public CustomPostTypeGenerator(
        ILiferayStructureRepository structureRepo,
        ILogger<CustomPostTypeGenerator> logger,
        string outputPath = "GeneratedPostTypes",
        GenerationMode mode = GenerationMode.ACF_JSON)
    {
        _structureRepo = structureRepo;
        _logger = logger;
        _outputPath = outputPath;
        _mode = mode;
    }

    public async Task GenerateFromLiferayStructuresAsync(long groupId, CancellationToken ct)
    {
        _logger.LogInformation("Loading DDM Structures from Liferay for group {GroupId}...", groupId);
        _logger.LogInformation("Generation mode: {Mode}", _mode);

        var structures = await _structureRepo.GetAllStructuresAsync(groupId, ct);

        _logger.LogInformation("Found {Count} structures", structures.Count);

        if (structures.Count == 0)
        {
            _logger.LogWarning("No structures found for group {GroupId}", groupId);
            return;
        }

        // Ordina le strutture alfabeticamente per nome
        structures = structures.OrderBy(s => s.Name).ToList();

        // Crea directory output
        Directory.CreateDirectory(_outputPath);
        Directory.CreateDirectory(Path.Combine(_outputPath, "acf-json"));

        if (_mode == GenerationMode.PHP)
        {
            Directory.CreateDirectory(Path.Combine(_outputPath, "post-types"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "taxonomies"));
        }

        // Raggruppa per ClassNameId e calcola posizioni menu
        var groupedStructures = structures.GroupBy(s => s.ClassNameId).ToList();
        var menuPositions = new Dictionary<string, int>();

        foreach (var group in groupedStructures)
        {
            int position = 0;
            foreach (var structure in group.OrderBy(s => s.Name))
            {
                menuPositions[structure.StructureKey] = position++;
            }
        }

        foreach (var structure in structures)
        {
            try
            {
                // Estrai il nome leggibile dall'XML
                var displayName = (structure.Name);
                var slug = SanitizeSlug(structure.StructureKey);
                var readableName = SanitizeFileName(displayName) + "-" + slug;
                var menuPosition = menuPositions[structure.StructureKey];

                _logger.LogInformation("Processing structure: {Name} ({Key}) -> {ReadableName}", displayName, structure.StructureKey, readableName);

                // Genera ACF JSON per Custom Fields
                var acfJson = await GenerateACFJsonAsync(structure, ct);
                var acfFileName = Path.Combine(_outputPath, "acf-json", $"fields-{slug}.json");
                await File.WriteAllTextAsync(acfFileName, acfJson, ct);
                _logger.LogInformation("  ✓ ACF Fields JSON: {File}", acfFileName);

                if (_mode == GenerationMode.ACF_JSON)
                {
                    // Genera ACF JSON per Post Type
                    var postTypeJson = await GenerateACFPostTypeJsonAsync(structure, menuPosition, ct);
                    var postTypeFileName = Path.Combine(_outputPath, "acf-json", $"post-type-{slug}.json");
                    await File.WriteAllTextAsync(postTypeFileName, postTypeJson, ct);
                    _logger.LogInformation("  ✓ ACF Post Type JSON: {File}", postTypeFileName);

                    // Genera ACF JSON per Taxonomies
                    var categoryJson = await GenerateACFTaxonomyJsonAsync(structure, "category", ct);
                    var categoryFileName = Path.Combine(_outputPath, "acf-json", $"taxonomy-{slug}_category.json");
                    await File.WriteAllTextAsync(categoryFileName, categoryJson, ct);
                    _logger.LogInformation("  ✓ ACF Category JSON: {File}", categoryFileName);

                    var tagJson = await GenerateACFTaxonomyJsonAsync(structure, "tag", ct);
                    var tagFileName = Path.Combine(_outputPath, "acf-json", $"taxonomy-{slug}_tag.json");
                    await File.WriteAllTextAsync(tagFileName, tagJson, ct);
                    _logger.LogInformation("  ✓ ACF Tag JSON: {File}", tagFileName);
                }
                else // PHP Mode
                {
                    // Genera codice PHP per Custom Post Type
                    var phpCode = await GeneratePostTypeCodeAsync(structure, menuPosition, ct);
                    var phpFileName = Path.Combine(_outputPath, "post-types", $"{readableName}.php");
                    await File.WriteAllTextAsync(phpFileName, phpCode, ct);
                    _logger.LogInformation("  ✓ PHP Code: {File}", phpFileName);

                    // Genera Custom Taxonomy
                    var taxonomyCode = await GenerateTaxonomyCodeAsync(structure, ct);
                    var taxonomyFileName = Path.Combine(_outputPath, "taxonomies", $"{readableName}_taxonomy.php");
                    await File.WriteAllTextAsync(taxonomyFileName, taxonomyCode, ct);
                    _logger.LogInformation("  ✓ Taxonomy: {File}", taxonomyFileName);
                }

                // Genera README con istruzioni
                var readme = GenerateReadme(structure);
                var readmeFileName = Path.Combine(_outputPath, "acf-json", $"{readableName}_README.md");
                await File.WriteAllTextAsync(readmeFileName, readme, ct);
                _logger.LogInformation("  ✓ README: {File}", readmeFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate files for structure {Key}", structure.StructureKey);
            }
        }

        // Genera file master di installazione
        await GenerateMasterInstallScriptAsync(structures, ct);

        _logger.LogInformation("✓ Generation complete! Files saved to: {Path}", Path.GetFullPath(_outputPath));
        _logger.LogInformation("");
        _logger.LogInformation("Next steps:");
        _logger.LogInformation("1. Install and activate 'Advanced Custom Fields PRO' plugin");
        _logger.LogInformation("2. Copy 'acf-json' folder to your theme folder");

        if (_mode == GenerationMode.PHP)
        {
            _logger.LogInformation("3. Use cmn-custom-post-types.php or add files manually to functions.php");
        }
        else
        {
            _logger.LogInformation("3. ACF will automatically register Post Types and Taxonomies from JSON");
        }

        _logger.LogInformation("4. Flush permalinks: Settings → Permalinks → Save");
    }

    public async Task<string> GenerateACFJsonAsync(DDMStructure structure, CancellationToken ct)
    {
        var postTypeSlug = SanitizeSlug(structure.StructureKey);
        var groupKey = $"fields_{postTypeSlug}";

        var acfGroup = new
        {
            key = groupKey,
            title = $"{(structure.Name)} - Campi Personalizzati",
            fields = structure.Fields.Select((field, index) => ConvertToACFField(field, index)).ToArray(),
            location = new[]
            {
                new[]
                {
                    new
                    {
                        param = "post_type",
                        @operator = "==",
                        value = $"post_type_{postTypeSlug}"
                    }
                }
            },
            menu_order = 0,
            position = "normal",
            style = "default",
            label_placement = "top",
            instruction_placement = "label",
            hide_on_screen = "",
            active = true,
            description = structure.Description,
            show_in_rest = 1,
            display_title = ""
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        await Task.CompletedTask; // Placeholder per async
        return JsonSerializer.Serialize(acfGroup, options);
    }

    public async Task<string> GeneratePostTypeCodeAsync(DDMStructure structure, int menuPosition, CancellationToken ct)
    {
        var slug = SanitizeSlug(structure.StructureKey);
        var singularName = structure.Name;
        var pluralName = MakePlural(singularName);
        var parentMenuSlug = structure.GetParentMenuSlug();

        var php = new StringBuilder();
        php.AppendLine("<?php");
        php.AppendLine("/**");
        php.AppendLine($" * CMN Custom Post Types: {singularName}");
        php.AppendLine($" * Generated from Liferay DDMStructure: {structure.StructureKey}");
        php.AppendLine($" * Structure ID: {structure.StructureId}");
        php.AppendLine($" * ClassNameId: {structure.ClassNameId} ({structure.GetStructureTypeName()})");
        php.AppendLine(" *");
        php.AppendLine(" * Add this code to your theme's functions.php or create a custom plugin");
        php.AppendLine(" */");
        php.AppendLine();
        php.AppendLine("if (!defined('ABSPATH')) exit;");
        php.AppendLine();
        php.AppendLine($"function register_cpt_{slug}() {{");
        php.AppendLine("    $labels = array(");
        php.AppendLine($"        'name'                  => __('{pluralName}', 'textdomain'),");
        php.AppendLine($"        'singular_name'         => __('{singularName}', 'textdomain'),");
        php.AppendLine($"        'menu_name'             => __('{pluralName}', 'textdomain'),");
        php.AppendLine($"        'name_admin_bar'        => __('{singularName}', 'textdomain'),");
        php.AppendLine($"        'add_new'               => __('Aggiungi', 'textdomain'),");
        php.AppendLine($"        'add_new_item'          => __('Aggiungi {singularName}', 'textdomain'),");
        php.AppendLine($"        'new_item'              => __('Nuovo', 'textdomain'),");
        php.AppendLine($"        'edit_item'             => __('Modifica {singularName}', 'textdomain'),");
        php.AppendLine($"        'view_item'             => __('Visualizza {singularName}', 'textdomain'),");
        php.AppendLine($"        'all_items'             => __('{pluralName}', 'textdomain'),");
        php.AppendLine($"        'search_items'          => __('Cerca {pluralName}', 'textdomain'),");
        php.AppendLine($"        'parent_item_colon'     => __('Genitore {pluralName}:', 'textdomain'),");
        php.AppendLine($"        'not_found'             => __('Nessun {pluralName.ToLower()} trovato.', 'textdomain'),");
        php.AppendLine($"        'not_found_in_trash'    => __('Nessun {pluralName.ToLower()} trovato nel Cestino.', 'textdomain')");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine("    $args = array(");
        php.AppendLine("        'labels'             => $labels,");
        php.AppendLine("        'public'             => true,");
        php.AppendLine("        'publicly_queryable' => true,");
        php.AppendLine("        'show_ui'            => true,");
        php.AppendLine($"        'show_in_menu'       => '{parentMenuSlug}',");
        php.AppendLine("        'query_var'          => true,");
        php.AppendLine($"        'rewrite'            => array('slug' => '{slug}'),");
        php.AppendLine("        'capability_type'    => 'post',");
        php.AppendLine("        'has_archive'        => true,");
        php.AppendLine("        'hierarchical'       => false,");
        php.AppendLine($"        'menu_position'      => {menuPosition},");
        php.AppendLine("        'supports'           => array('title', 'author', 'thumbnail'),"); /* , 'editor', 'excerpt', 'comments' */
        php.AppendLine("        'show_in_rest'       => true,");
        php.AppendLine($"        'taxonomies'         => array('{slug}_category', '{slug}_tag'),");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine($"    register_post_type('{slug}', $args);");
        php.AppendLine("}");
        php.AppendLine($"add_action('init', 'register_cpt_{slug}');");

        await Task.CompletedTask;
        return php.ToString();
    }

    /// <summary>
    /// Genera il codice per la taxonomy personalizzata
    /// </summary>
    public async Task<string> GenerateTaxonomyCodeAsync(DDMStructure structure, CancellationToken ct)
    {
        var slug = SanitizeSlug(structure.StructureKey);
        var singularName = (structure.Name);
        var pluralName = MakePlural(singularName);

        var php = new StringBuilder();
        php.AppendLine("<?php");
        php.AppendLine("/**");
        php.AppendLine($" * CMN Custom Taxonomy for: {singularName}");
        php.AppendLine($" * Generated from Liferay DDMStructure: {structure.StructureKey}");
        php.AppendLine(" *");
        php.AppendLine(" * Hierarchical (Category-style) and Non-hierarchical (Tag-style) taxonomies");
        php.AppendLine(" */");
        php.AppendLine();
        php.AppendLine("if (!defined('ABSPATH')) exit;");
        php.AppendLine();

        // Taxonomy gerarchica (Category)
        php.AppendLine($"// Hierarchical taxonomy (like Categories)");
        php.AppendLine($"function register_taxonomy_{slug}_category() {{");
        php.AppendLine("    $labels = array(");
        php.AppendLine($"        'name'              => __('{singularName} Categories', 'textdomain'),");
        php.AppendLine($"        'singular_name'     => __('{singularName} Category', 'textdomain'),");
        php.AppendLine($"        'search_items'      => __('Cerca Categorie', 'textdomain'),");
        php.AppendLine($"        'all_items'         => __('Tutte le Categorie', 'textdomain'),");
        php.AppendLine($"        'parent_item'       => __('Categoria Genitore', 'textdomain'),");
        php.AppendLine($"        'parent_item_colon' => __('Categoria Genitore:', 'textdomain'),");
        php.AppendLine($"        'edit_item'         => __('Modifica Categoria', 'textdomain'),");
        php.AppendLine($"        'update_item'       => __('Aggiorna Categoria', 'textdomain'),");
        php.AppendLine($"        'add_new_item'      => __('Aggiungi Nuova Categoria', 'textdomain'),");
        php.AppendLine($"        'new_item_name'     => __('Nome Nuova Categoria', 'textdomain'),");
        php.AppendLine($"        'menu_name'         => __('Categorie', 'textdomain'),");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine("    $args = array(");
        php.AppendLine("        'hierarchical'      => true,");
        php.AppendLine("        'labels'            => $labels,");
        php.AppendLine("        'show_ui'           => true,");
        php.AppendLine("        'show_admin_column' => true,");
        php.AppendLine("        'query_var'         => true,");
        php.AppendLine($"        'rewrite'           => array('slug' => '{slug}-category'),");
        php.AppendLine("        'show_in_rest'      => true,");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine($"    register_taxonomy('{slug}_category', array('{slug}'), $args);");
        php.AppendLine("}");
        php.AppendLine($"add_action('init', 'register_taxonomy_{slug}_category');");
        php.AppendLine();

        // Taxonomy non gerarchica (Tags)
        php.AppendLine($"// Non-hierarchical taxonomy (like Tags)");
        php.AppendLine($"function register_taxonomy_{slug}_tag() {{");
        php.AppendLine("    $labels = array(");
        php.AppendLine($"        'name'                       => __('{singularName} Tags', 'textdomain'),");
        php.AppendLine($"        'singular_name'              => __('{singularName} Tag', 'textdomain'),");
        php.AppendLine($"        'search_items'               => __('Cerca Tags', 'textdomain'),");
        php.AppendLine($"        'popular_items'              => __('Categorie Popolari', 'textdomain'),");
        php.AppendLine($"        'all_items'                  => __('Tutti i Tags', 'textdomain'),");
        php.AppendLine($"        'edit_item'                  => __('Modifica Tag', 'textdomain'),");
        php.AppendLine($"        'update_item'                => __('Aggiorna Tag', 'textdomain'),");
        php.AppendLine($"        'add_new_item'               => __('Aggiungi Nuovo Tag', 'textdomain'),");
        php.AppendLine($"        'new_item_name'              => __('Nome Nuovo Tag', 'textdomain'),");
        php.AppendLine($"        'separate_items_with_commas' => __('Separa i tag con le virgole', 'textdomain'),");
        php.AppendLine($"        'add_or_remove_items'        => __('Aggiungi o rimuovi tag', 'textdomain'),");
        php.AppendLine($"        'choose_from_most_used'      => __('Scegli dai tag più usati', 'textdomain'),");
        php.AppendLine($"        'menu_name'                  => __('Tag', 'textdomain'),");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine("    $args = array(");
        php.AppendLine("        'hierarchical'      => false,");
        php.AppendLine("        'labels'            => $labels,");
        php.AppendLine("        'show_ui'           => true,");
        php.AppendLine("        'show_admin_column' => true,");
        php.AppendLine("        'query_var'         => true,");
        php.AppendLine($"        'rewrite'           => array('slug' => '{slug}-tag'),");
        php.AppendLine("        'show_in_rest'      => true,");
        php.AppendLine("    );");
        php.AppendLine();
        php.AppendLine($"    register_taxonomy('{slug}_tag', array('{slug}'), $args);");
        php.AppendLine("}");
        php.AppendLine($"add_action('init', 'register_taxonomy_{slug}_tag');");

        await Task.CompletedTask;
        return php.ToString();
    }

    /// <summary>
    /// Genera ACF JSON per Custom Post Type (ACF PRO 6.1+)
    /// </summary>
    private async Task<string> GenerateACFPostTypeJsonAsync(DDMStructure structure, int menuPosition, CancellationToken ct)
    {
        var slug = SanitizeSlug(structure.StructureKey);
        var singularName = structure.Name;
        var pluralName = MakePlural(singularName);
        var parentMenuSlug = structure.GetParentMenuSlug();

        var acfPostType = new Dictionary<string, object>
        {
            ["key"] = $"post_type_{slug}",
            ["title"] = structure.Name,
            ["menu_order"] = 0,
            ["active"] = true,
            ["post_type"] = $"post_type_{slug}",
            ["advanced_configuration"] = false,
            ["import_source"] = "",
            ["import_date"] = "",
            ["label"] = pluralName,
            ["description"] = structure.Description ?? "",
            ["labels"] = new Dictionary<string, string>
            {
                ["name"] = pluralName,
                ["singular_name"] = singularName,
                ["menu_name"] = pluralName,
                ["edit_item"] = $"Modifica {singularName}",
                ["view_item"] = $"Visualizza {singularName}",
                ["view_items"] = $"Visualizza {pluralName}",
                ["add_new_item"] = $"Aggiungi {singularName}",
                ["add_new"] = "Aggiungi",
                ["new_item"] = "Nuovo",
                ["parent_item_colon"] = $"{singularName} genitore:",
                ["search_items"] = $"Cerca {pluralName}",
                ["not_found"] = $"Nessun {singularName} trovato.",
                ["not_found_in_trash"] = $"Nessun {singularName} trovato nel Cestino.",
                ["archives"] = $"Archivi {pluralName}",
                ["attributes"] = $"Attributi {singularName}",
                ["featured_image"] = "",
                ["set_featured_image"] = "",
                ["remove_featured_image"] = "",
                ["use_featured_image"] = "",
                ["insert_into_item"] = $"Inserisci in {singularName}",
                ["uploaded_to_this_item"] = $"Caricato in questo/a {singularName}",
                ["filter_items_list"] = $"Filtrare l'elenco di {pluralName}",
                ["filter_by_date"] = $"Filtra {pluralName} per data",
                ["items_list_navigation"] = $"Navigazione elenco {pluralName}",
                ["items_list"] = $"Elenco {pluralName}",
                ["item_published"] = $"{singularName} pubblicato\\/a.",
                ["item_published_privately"] = $"{singularName} pubblicato\\/a privatamente.",
                ["item_reverted_to_draft"] = $"{singularName} riconvertito\\/a in bozza.",
                ["item_scheduled"] = $"{singularName} programmato\\/a.",
                ["item_updated"] = $"{singularName} aggiornato\\/a.",
                ["item_link"] = $"Link {singularName}",
                ["item_link_description"] = $"Un link a un\\/a {singularName}."
            },
            ["description"] = "",
            ["public"] = true,
            ["hierarchical"] = false,
            ["exclude_from_search"] = false,
            ["publicly_queryable"] = true,
            ["show_ui"] = true,
            ["show_in_menu"] = true,
            ["admin_menu_parent"] = parentMenuSlug,
            ["show_in_admin_bar"] = true,
            ["show_in_nav_menus"] = true,
            ["show_in_rest"] = true,
            ["rest_base"] = 45411,
            ["rest_namespace"] = "wp\\/v2",
            ["rest_controller_class"] = "WP_REST_Posts_Controller",
            ["menu_position"] = menuPosition,
            ["menu_icon"] = new Dictionary<string, string>
            {
                ["type"] = "dashicons",
                ["value"] = "dashicons-admin-post"
            },
            ["rename_capabilities"] = false,
            ["singular_capability_name"] = "post",
            ["plural_capability_name"] = "posts",
            ["supports"] = new[] { "title", "author", "thumbnail" },
            ["taxonomies"] = new[] { $"taxonomy_{slug}_category", $"taxonomy_{slug}_tag" },
            ["has_archive"] = true,
            ["has_archive_slug"] = "",
            ["rewrite"] = new Dictionary<string, string>
            {
                ["permalink_rewrite"] = "post_type_key",
                ["with_front"] = "1",
                ["feeds"] = "1",
                ["pages"] = "1"
            },
            ["query_var"] = "post_type_key",
            ["query_var_name"] = "",
            ["can_export"] = true,
            ["delete_with_user"] = false,
            ["register_meta_box_cb"] = "",
            ["enter_title_here"] = ""
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await Task.CompletedTask;
        return JsonSerializer.Serialize(acfPostType, options);
    }

    /// <summary>
    /// Genera ACF JSON per Taxonomy (ACF PRO 6.1+)
    /// </summary>
    private async Task<string> GenerateACFTaxonomyJsonAsync(DDMStructure structure, string type, CancellationToken ct)
    {
        var slug = SanitizeSlug(structure.StructureKey);
        var singularName = structure.Name;
        var taxonomySlug = $"{slug}_{type}";
        var isHierarchical = type == "category";

        // Crea labels dinamicamente in base al tipo
        var labels = new Dictionary<string, string>();

        if (isHierarchical)
        {
            labels["name"] = $"{singularName} Categorie";
            labels["singular_name"] = $"{singularName} Categoria";
            labels["menu_name"] = "Categorie";
            labels["all_items"] = "Tutte le Categorie";
            labels["edit_item"] = "Modifica Categoria";
            labels["view_item"] = "Visualizza Categoria";
            labels["update_item"] = "Aggiorna Categoria";
            labels["add_new_item"] = "Aggiungi Nuova Categoria";
            labels["new_item_name"] = "Nome Nuova Categoria";
            labels["parent_item"] = "Categoria Genitore";
            labels["parent_item_colon"] = "Categoria Genitore:";
            labels["search_items"] = "Cerca Categorie";
            labels["most_used"] = "";
            labels["not_found"] = "Nessuna categoria trovata.";
            labels["no_terms"] = "";
            labels["name_field_description"] = "";
            labels["slug_field_description"] = "";
            labels["parent_field_description"] = "";
            labels["desc_field_description"] = "";
            labels["filter_by_item"] = $"Filtra per {singularName} categoria";
            labels["items_list_navigation"] = "";
            labels["items_list"] = "";
            labels["back_to_items"] = "";
            labels["item_link"] = $"Link {singularName} Categoria";
            labels["item_link_description"] = $"Un link a {singularName} categoria";
        }
        else
        {
            labels["name"] = $"{singularName} Tag";
            labels["singular_name"] = $"{singularName} Tag";
            labels["menu_name"] = "Tag";
            labels["all_items"] = "Tutti i Tag";
            labels["edit_item"] = "Modifica Tag";
            labels["view_item"] = "Visualizza Tag";
            labels["update_item"] = "Aggiorna Tag";
            labels["add_new_item"] = "Aggiungi Nuovo Tag";
            labels["new_item_name"] = "Nome Nuovo Tag";
            labels["parent_item"] = "";
            labels["parent_item_colon"] = "";
            labels["search_items"] = "Cerca Tag";
            labels["popular_items"] = "Tag Popolari";
            labels["separate_items_with_commas"] = "Separa i tag con le virgole";
            labels["add_or_remove_items"] = "Aggiungi o rimuovi tag";
            labels["choose_from_most_used"] = "Scegli dai tag più usati";
            labels["not_found"] = "Nessun tag trovato.";
        }

        var acfTaxonomy = new Dictionary<string, object>
        {
            ["key"] = $"taxonomy_{taxonomySlug}",
            ["title"] = structure.Name + (isHierarchical ? " Categoria" : " Tag"),
            ["menu_order"] = 0,
            ["active"] = true,
            ["taxonomy"] = $"taxonomy_{taxonomySlug}",
            ["object_type"] = new[] { $"post_type_{slug}" },
            ["advanced_configuration"] = 1,
            ["import_source"] = "",
            ["import_date"] = "",

            ["label"] = isHierarchical ? "Categorie" : "Tag",
            ["description"] = "",
            ["labels"] = labels,
            ["description"] = "",
            ["capabilities"] = new Dictionary<string, string>
            {
                ["manage_terms"] = "manage_categories",
                ["edit_terms"] = "manage_categories",
                ["delete_terms"] = "manage_categories",
                ["assign_terms"] = "edit_posts"
            },
            ["public"] = "1",
            ["publicly_queryable"] = 1,
            ["hierarchical"] = isHierarchical ? 1 : 0,
            ["show_ui"] = 1,
            ["show_in_menu"] = 1,
            ["show_in_nav_menus"] = 1,
            ["show_in_rest"] = 1,
            ["rest_base"] = taxonomySlug,
            ["rest_namespace"] = "wp\\/v2",
            ["rest_controller_class"] = "WP_REST_Terms_Controller",
            ["show_tagcloud"] = isHierarchical ? 0 : 1,
            ["show_in_quick_edit"] = 1,
            ["show_admin_column"] = 1,
            ["object_types"] = new[] { slug },
            ["rewrite"] = new Dictionary<string, object>
            {
                ["permalink_rewrite"] = "taxonomy_key",
                ["with_front"] = 1,
                ["rewrite_hierarchical"] = isHierarchical ? 1 : 0
            },
            ["query_var"] = "post_type_key",
            ["query_var_name"] = "",
            ["default_term"] = new Dictionary<string, object>
            {
                ["default_term_enabled"] = "0"
            },
            ["sort"] = 0,
            ["meta_box"] = "default",
            ["meta_box_cb"] = "",
            ["meta_box_sanitize_cb"] = ""
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await Task.CompletedTask;
        return JsonSerializer.Serialize(acfTaxonomy, options);
    }

    private object ConvertToACFField(DDMField field, int order)
    {
        var fieldKey = $"field_{SanitizeSlug(field.Name)}";
        var acfType = MapLiferayTypeToACF(field.Type);

        var acfField = new Dictionary<string, object>
        {
            ["key"] = fieldKey,
            ["label"] = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label,
            ["name"] = SanitizeSlug(field.Name),
            ["aria-label"] = "",
            ["type"] = acfType,
            ["instructions"] = field.Tip,
            ["required"] = field.Required ? 1 : 0,
            ["conditional_logic"] = 0,
            ["wrapper"] = new Dictionary<string, object>
            {
                ["width"] = "",
                ["class"] = "",
                ["id"] = ""
            },
            ["default_value"] = "",
            ["maxlength"] = "",
            ["placeholder"] = "",
            ["prepend"] = "",
            ["append"] = ""
        };

        // Aggiungi proprietà specifiche per tipo
        switch (acfType)
        {
            case "select":
            case "radio":
            case "checkbox":
                acfField["choices"] = field.Options.ToDictionary(o => o.Value, o => o.Label);
                acfField["default_value"] = "";
                acfField["allow_null"] = field.Required ? 0 : 1;
                acfField["multiple"] = field.Repeatable ? 1 : 0;
                break;

            case "textarea":
                acfField["default_value"] = "";
                acfField["new_lines"] = ""; // wpautop
                acfField["maxlength"] = "";
                acfField["rows"] = 4;
                break;

            case "wysiwyg":
                acfField["default_value"] = "";
                acfField["tabs"] = "all";
                acfField["toolbar"] = "full";
                acfField["media_upload"] = 1;
                acfField["delay"] = 0;
                break;

            case "number":
                acfField["default_value"] = "";
                acfField["min"] = "";
                acfField["max"] = "";
                acfField["step"] = "";
                break;

            case "date_picker":
                acfField["display_format"] = "d/m/Y";
                acfField["return_format"] = "d/m/Y";
                acfField["first_day"] = 1;
                break;

            case "true_false":
                acfField["default_value"] = 0;
                acfField["ui"] = 1;
                acfField["ui_on_text"] = "";
                acfField["ui_off_text"] = "";
                break;

            case "file":
            case "image":
                acfField["return_format"] = "array";
                acfField["library"] = "all";
                break;
        }

        return acfField;
    }

    private static string MapLiferayTypeToACF(string liferayType)
    {
        return liferayType.ToLowerInvariant() switch
        {
            "text" => "text",
            "textarea" => "textarea",
            "html" or "text-html" or "ddm-text-html" => "wysiwyg",
            "select" => "select",
            "radio" => "radio",
            "checkbox" => "checkbox",
            "number" or "integer" or "ddm-integer" => "number",
            "decimal" or "ddm-decimal" => "number",
            "date" or "ddm-date" => "date_picker",
            "boolean" => "true_false",
            "image" or "ddm-image" => "image",
            "document-library" or "ddm-document-library" => "file",
            "link-to-page" => "link",
            "geolocation" or "ddm-geolocation" => "google_map",
            _ => "text" // Fallback
        };
    }

    private string GenerateReadme(DDMStructure structure)
    {
        var sb = new StringBuilder();
        var slug = SanitizeSlug(structure.StructureKey);

        sb.AppendLine($"# {structure.Name} - CMN Custom Post Type");
        sb.AppendLine();
        sb.AppendLine($"**Generation Mode:** `{_mode}`");
        sb.AppendLine($"**Generated from Liferay Structure:** `{structure.StructureKey}`");
        sb.AppendLine($"**Structure ID:** `{structure.StructureId}`");
        sb.AppendLine($"**ClassNameId:** `{structure.ClassNameId}` - {structure.GetStructureTypeName()}");
        sb.AppendLine($"**Parent Menu:** {structure.GetStructureTypeName()}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(structure.Description))
        {
            sb.AppendLine("## Description");
            sb.AppendLine();
            sb.AppendLine(structure.Description);
            sb.AppendLine();
        }

        sb.AppendLine("## Installation");
        sb.AppendLine();
        sb.AppendLine("### 1. Install Advanced Custom Fields PRO");
        sb.AppendLine();
        sb.AppendLine("### 2. Copy ACF JSON");
        sb.AppendLine();
        sb.AppendLine("Copy all JSON files from `acf-json/` folder to your theme's `acf-json/` folder");
        sb.AppendLine();

        if (_mode == GenerationMode.ACF_JSON)
        {
            sb.AppendLine("### 3. ACF JSON Mode");
            sb.AppendLine();
            sb.AppendLine("**ACF PRO 6.1+** will automatically:");
            sb.AppendLine("- Register the Custom Post Type from `post-type-*.json`");
            sb.AppendLine("- Register the Taxonomies from `taxonomy-*.json`");
            sb.AppendLine("- Register the Custom Fields from `group-*.json`");
            sb.AppendLine();
            sb.AppendLine("No PHP code needed! Just install the plugin `cmn-custom-post-types.php`");
        }
        else
        {
            sb.AppendLine("### 3. PHP Mode - Register Custom Post Type");
            sb.AppendLine();
            sb.AppendLine($"Add the code from `post-types/{slug}.php` to your theme's `functions.php`");
            sb.AppendLine("Or use the plugin `cmn-custom-post-types.php` which includes everything");
        }

        sb.AppendLine();
        sb.AppendLine("**Note:** This post type will appear under the **{0}** menu in WordPress admin.");
        sb.AppendLine();
        sb.AppendLine("### 4. Flush Rewrite Rules");
        sb.AppendLine();
        sb.AppendLine("Go to **WordPress Admin → Settings → Permalinks** and click \"Save Changes\"");
        sb.AppendLine();

        sb.AppendLine("## Custom Fields");
        sb.AppendLine();
        sb.AppendLine($"This post type has **{structure.Fields.Count}** custom fields:");
        sb.AppendLine();
        sb.AppendLine("| Field Name | Type | Required | Description |");
        sb.AppendLine("|------------|------|----------|-------------|");

        foreach (var field in structure.Fields)
        {
            var required = field.Required ? "✓" : "";
            var tip = string.IsNullOrEmpty(field.Tip) ? "-" : field.Tip;
            sb.AppendLine($"| {field.Label} | `{field.Type}` | {required} | {tip} |");
        }

        sb.AppendLine();

        var readme = sb.ToString();
        return readme.Replace("**Note:** This post type will appear under the **{0}** menu in WordPress admin.",
                             $"**Note:** This post type will appear under the **{structure.GetStructureTypeName()}** menu in WordPress admin.");
    }

    private async Task GenerateMasterInstallScriptAsync(List<DDMStructure> structures, CancellationToken ct)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# WordPress Custom Post Types Installation Guide");
        sb.AppendLine();
        sb.AppendLine($"**Generation Mode:** `{_mode}`");
        sb.AppendLine($"**Generated on:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**Total structures:** {structures.Count}");
        sb.AppendLine();

        if (_mode == GenerationMode.ACF_JSON)
        {
            sb.AppendLine("## 🎯 ACF JSON Mode");
            sb.AppendLine();
            sb.AppendLine("This generation uses **ACF PRO 6.1+ JSON format** for:");
            sb.AppendLine("- ✅ Custom Post Types (`post-type-*.json`)");
            sb.AppendLine("- ✅ Taxonomies (`taxonomy-*.json`)");
            sb.AppendLine("- ✅ Custom Fields (`group-*.json`)");
            sb.AppendLine();
            sb.AppendLine("**Advantages:**");
            sb.AppendLine("- No PHP code for post types and taxonomies");
            sb.AppendLine("- Visual management via ACF UI");
            sb.AppendLine("- Easy sync between environments");
            sb.AppendLine("- Version control friendly");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("## 📝 PHP Mode");
            sb.AppendLine();
            sb.AppendLine("This generation uses **traditional PHP code** for:");
            sb.AppendLine("- Post Types (`post-types/*.php`)");
            sb.AppendLine("- Taxonomies (`taxonomies/*.php`)");
            sb.AppendLine("- Custom Fields via ACF JSON (`acf-json/*.json`)");
            sb.AppendLine();
        }

        sb.AppendLine("## Quick Installation");
        sb.AppendLine();
        sb.AppendLine("1. **Install ACF PRO** - Version 6.1+ required for ACF JSON mode");
        sb.AppendLine("2. **Copy ACF JSON** - Copy all files from `acf-json/` to your theme's `acf-json/` folder");

        if (_mode == GenerationMode.PHP)
        {
            sb.AppendLine("3. **Install Plugin** - Upload and activate `cmn-custom-post-types.php`");
            sb.AppendLine("   - Or manually copy PHP files from `post-types/` and `taxonomies/` to your theme");
        }
        else
        {
            sb.AppendLine("3. **Install Plugin** - Upload and activate `cmn-custom-post-types.php` (only for parent menus)");
            sb.AppendLine("   - ACF will automatically register post types and taxonomies from JSON");
        }

        sb.AppendLine("4. **Flush Permalinks** - Go to Settings → Permalinks → Save");
        sb.AppendLine();

        sb.AppendLine("## Parent Menus");
        sb.AppendLine();
        sb.AppendLine("Custom Post Types are organized under parent menus based on their Liferay classNameId:");
        sb.AppendLine();

        var groupedStructures = structures.GroupBy(s => s.ClassNameId).ToList();
        foreach (var group in groupedStructures)
        {
            var sample = group.First();
            sb.AppendLine($"### {sample.GetStructureTypeName()} (ClassNameId: {sample.ClassNameId})");
            sb.AppendLine($"- **Menu Slug:** `{sample.GetParentMenuSlug()}`");
            sb.AppendLine($"- **Icon:** `{sample.GetParentMenuIcon()}`");
            sb.AppendLine($"- **Post Types Count:** {group.Count()}");
            sb.AppendLine();

            foreach (var structure in group)
            {
                var slug = SanitizeSlug(structure.StructureKey);
                var name = (structure.Name);
                sb.AppendLine($"  - **{name}** (`{slug}`) - {structure.Fields.Count} fields");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## All Post Types");
        sb.AppendLine();

        foreach (var structure in structures)
        {
            var slug = SanitizeSlug(structure.StructureKey);
            sb.AppendLine($"### {(structure.Name)}");
            sb.AppendLine($"- **Slug:** `{slug}`");
            sb.AppendLine($"- **Fields:** {structure.Fields.Count}");
            sb.AppendLine($"- **Parent Menu:** {structure.GetStructureTypeName()}");
            sb.AppendLine($"- **ClassNameId:** {structure.ClassNameId}");
            sb.AppendLine();
        }

        var masterReadme = Path.Combine(_outputPath, "INSTALLATION_GUIDE.md");
        await File.WriteAllTextAsync(masterReadme, sb.ToString(), ct);

        // Genera anche template per custom plugin
        await GenerateCustomPluginTemplateAsync(structures, ct);
    }

    private async Task GenerateCustomPluginTemplateAsync(List<DDMStructure> structures, CancellationToken ct)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<?php");
        sb.AppendLine("/**");
        sb.AppendLine(" * Plugin Name: CMN Custom Post Types");
        sb.AppendLine(" * Plugin URI: ");
        sb.AppendLine(" * Description: Abilita le nuove tipologie di Post (Articoli) utili alla Città Metropolitana di Napoli.");
        sb.AppendLine(" * Version: 1.0.0");
        sb.AppendLine(" * Author: Auriemma Francesco");
        sb.AppendLine(" * Author URI: https://apsnet.it");
        sb.AppendLine(" * License: MIT");
        sb.AppendLine(" * Text Domain: cmn-custom-post-types");
        sb.AppendLine($" * Generation Mode: {_mode}");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine("if (!defined('ABSPATH')) exit;");
        sb.AppendLine();

        if (_mode == GenerationMode.ACF_JSON)
        {
            // Modalità ACF JSON: solo parent menus e ACF JSON configuration
            sb.AppendLine("// ACF JSON Mode: Post Types and Taxonomies are loaded from JSON");
            sb.AppendLine();

            // Genera i menu parent per ogni tipo di struttura
            var parentMenus = structures
                .GroupBy(s => s.ClassNameId)
                .Select(g => g.First())
                .ToList();

            foreach (var structure in parentMenus)
            {
                var parentSlug = structure.GetParentMenuSlug();
                var parentName = structure.GetStructureTypeName();
                var parentIcon = structure.GetParentMenuIcon();

                sb.AppendLine($"// Register parent menu for {parentName}");
                sb.AppendLine($"function register_parent_menu_{parentSlug}() {{");
                sb.AppendLine($"    add_menu_page(");
                sb.AppendLine($"        __('{parentName}', 'cmn-custom-post-types'),");
                sb.AppendLine($"        __('{parentName}', 'cmn-custom-post-types'),");
                sb.AppendLine($"        'edit_posts',");
                sb.AppendLine($"        '{parentSlug}',");
                sb.AppendLine($"        '',");
                sb.AppendLine($"        '{parentIcon}',");
                sb.AppendLine($"        25");
                sb.AppendLine($"    );");
                sb.AppendLine("}");
                sb.AppendLine($"add_action('admin_menu', 'register_parent_menu_{parentSlug}');");
                sb.AppendLine();
            }

            sb.AppendLine("// ACF JSON save point");
            sb.AppendLine("add_filter('acf/settings/save_json', function($path) {");
            sb.AppendLine("    return plugin_dir_path(__FILE__) . 'acf-json';");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("// ACF JSON load point");
            sb.AppendLine("add_filter('acf/settings/load_json', function($paths) {");
            sb.AppendLine("    $paths[] = plugin_dir_path(__FILE__) . 'acf-json';");
            sb.AppendLine("    return $paths;");
            sb.AppendLine("});");
        }
        else // PHP Mode
        {
            // Modalità PHP: carica i file PHP
            sb.AppendLine("// PHP Mode: Post Types and Taxonomies are loaded from PHP files");
            sb.AppendLine();

            // Genera i menu parent per ogni tipo di struttura
            var parentMenus = structures
                .GroupBy(s => s.ClassNameId)
                .Select(g => g.First())
                .ToList();

            foreach (var structure in parentMenus)
            {
                var parentSlug = structure.GetParentMenuSlug();
                var parentName = structure.GetStructureTypeName();
                var parentIcon = structure.GetParentMenuIcon();

                sb.AppendLine($"// Register parent menu for {parentName}");
                sb.AppendLine($"function register_parent_menu_{parentSlug}() {{");
                sb.AppendLine($"    add_menu_page(");
                sb.AppendLine($"        __('{parentName}', 'cmn-custom-post-types'),");
                sb.AppendLine($"        __('{parentName}', 'cmn-custom-post-types'),");
                sb.AppendLine($"        'edit_posts',");
                sb.AppendLine($"        '{parentSlug}',");
                sb.AppendLine($"        '',");
                sb.AppendLine($"        '{parentIcon}',");
                sb.AppendLine($"        25");
                sb.AppendLine($"    );");
                sb.AppendLine("}");
                sb.AppendLine($"add_action('admin_menu', 'register_parent_menu_{parentSlug}');");
                sb.AppendLine();
            }

            sb.AppendLine("// Load all custom post types");
            foreach (var structure in structures)
            {
                var slug = SanitizeSlug(structure.StructureKey);
                var readableName = SanitizeFileName((structure.Name));
                sb.AppendLine($"require_once plugin_dir_path(__FILE__) . 'post-types/{readableName}-{slug}.php';");
            }

            sb.AppendLine();
            sb.AppendLine("// Load all custom taxonomies");
            foreach (var structure in structures)
            {
                var slug = SanitizeSlug(structure.StructureKey);
                var readableName = SanitizeFileName((structure.Name));
                sb.AppendLine($"require_once plugin_dir_path(__FILE__) . 'taxonomies/{readableName}-{slug}_taxonomy.php';");
            }

            sb.AppendLine();
            sb.AppendLine("// ACF JSON save point");
            sb.AppendLine("add_filter('acf/settings/save_json', function($path) {");
            sb.AppendLine("    return plugin_dir_path(__FILE__) . 'acf-json';");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("// ACF JSON load point");
            sb.AppendLine("add_filter('acf/settings/load_json', function($paths) {");
            sb.AppendLine("    $paths[] = plugin_dir_path(__FILE__) . 'acf-json';");
            sb.AppendLine("    return $paths;");
            sb.AppendLine("});");
        }

        sb.AppendLine();
        sb.AppendLine("// Flush rewrite rules on activation");
        sb.AppendLine("register_activation_hook(__FILE__, function() {");
        sb.AppendLine("    // Trigger init to register post types and taxonomies");
        sb.AppendLine("    do_action('init');");
        sb.AppendLine("    // Flush rewrite rules");
        sb.AppendLine("    flush_rewrite_rules();");
        sb.AppendLine("});");
        sb.AppendLine();
        sb.AppendLine("// Flush rewrite rules on deactivation");
        sb.AppendLine("register_deactivation_hook(__FILE__, function() {");
        sb.AppendLine("    flush_rewrite_rules();");
        sb.AppendLine("});");

        var pluginTemplate = Path.Combine(_outputPath, "cmn-custom-post-types.php");
        await File.WriteAllTextAsync(pluginTemplate, sb.ToString(), ct);
    }

    /// <summary>
    /// Crea una versione plurale semplificata del nome
    /// </summary>
    private static string MakePlural(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Regole di pluralizzazione italiana

        // Parole che terminano con consonante o vocale accentata -> invariabili
        if (name.Length > 0 && !"aeiou".Contains(name[^1], StringComparison.OrdinalIgnoreCase))
            return name;

        // Parole che terminano con -à, -è, -ì, -ò, -ù -> invariabili
        if (name.EndsWith("à") || name.EndsWith("è") || name.EndsWith("ì") ||
            name.EndsWith("ò") || name.EndsWith("ù"))
            return name;

        // Parole che terminano con -ca, -ga -> -che, -ghe
        if (name.EndsWith("ca", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "he"; // amica -> amiche
        if (name.EndsWith("ga", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "he"; // collega -> colleghe

        // Parole che terminano con -co, -go
        if (name.EndsWith("co", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "hi"; // banco -> banchi, medico -> medici (semplificato)
        if (name.EndsWith("go", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "hi"; // catalogo -> cataloghi

        // Parole che terminano con -cia, -gia
        if (name.EndsWith("cia", StringComparison.OrdinalIgnoreCase))
        {
            if (name.Length >= 4 && !"aeiou".Contains(name[^4], StringComparison.OrdinalIgnoreCase))
                return name[..^2] + "e"; // provincia -> province
            return name[..^1] + "e"; // camicia -> camicie
        }
        if (name.EndsWith("gia", StringComparison.OrdinalIgnoreCase))
        {
            if (name.Length >= 4 && !"aeiou".Contains(name[^4], StringComparison.OrdinalIgnoreCase))
                return name[..^2] + "e"; // pioggia -> piogge
            return name[..^1] + "e"; // valigia -> valigie
        }

        // Parole che terminano con -cio, -gio
        if (name.EndsWith("cio", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "i"; // bacio -> baci
        if (name.EndsWith("gio", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "i"; // orologio -> orologi

        // Parole che terminano con -io (atono) -> -i
        if (name.EndsWith("io", StringComparison.OrdinalIgnoreCase))
            return name[..^1]; // stadio -> stadi, armadio -> armadi

        // Parole che terminano con -a -> -e (femminile singolare)
        if (name.EndsWith("a", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "e"; // casa -> case, pagina -> pagine

        // Parole che terminano con -o -> -i (maschile singolare)
        if (name.EndsWith("o", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "i"; // documento -> documenti, articolo -> articoli

        // Parole che terminano con -e -> -i (maschile/femminile singolare)
        if (name.EndsWith("e", StringComparison.OrdinalIgnoreCase))
            return name[..^1] + "i"; // chiave -> chiavi, cane -> cani

        // Parole che terminano con -i -> invariabili (già plurali o straniere)
        if (name.EndsWith("i", StringComparison.OrdinalIgnoreCase))
            return name; // crisi -> crisi, analisi -> analisi

        // Fallback per parole non coperte dalle regole (es. parole straniere)
        return name;
    }

    /// <summary>
    /// Escape stringhe PHP per evitare problemi con apici
    /// </summary>
    private static string EscapePhpString(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }

    private static string SanitizeSlug(string input)
    {
        return input
            .ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_")
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString();
    }

    private static string SanitizeFileName(string input)
    {
        return string.Join("_", input.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_").Replace("\'", "_").Replace(".", "_");
    }
}
