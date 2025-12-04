# Modulo Generale copia - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `3438606`
**Structure ID:** `3438607`
**ClassNameId:** `10109` - CMN - Contenuti
**Parent Menu:** CMN - Contenuti

## Installation

### 1. Install Advanced Custom Fields PRO

### 2. Copy ACF JSON

Copy all JSON files from `acf-json/` folder to your theme's `acf-json/` folder

### 3. ACF JSON Mode

**ACF PRO 6.1+** will automatically:
- Register the Custom Post Type from `post-type-*.json`
- Register the Taxonomies from `taxonomy-*.json`
- Register the Custom Fields from `group-*.json`

No PHP code needed! Just install the plugin `cmn-custom-post-types.php`

**Note:** This post type will appear under the **CMN - Contenuti** menu in WordPress admin.

### 4. Flush Rewrite Rules

Go to **WordPress Admin → Settings → Permalinks** and click "Save Changes"

## Custom Fields

This post type has **10** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Sottotitolo | `text` |  | - |
| Data | `ddm-date` |  | - |
| Data di pubblicazione | `checkbox` |  | - |
| Proprietà | `text` |  | - |
| Testo | `text` |  | - |
| Contenuto breve | `ddm-text-html` |  | - |
| Contenuto Esteso | `ddm-text-html` |  | - |
| Nome Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Allegato Nascosto | `checkbox` |  | - |

