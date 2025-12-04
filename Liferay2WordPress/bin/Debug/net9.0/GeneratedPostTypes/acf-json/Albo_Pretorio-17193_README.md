# Albo Pretorio - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `17193`
**Structure ID:** `17194`
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

This post type has **13** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Crea nuovo numero di pubblicazione | `checkbox` |  | - |
| Contenuto | `ddm-text-html` |  | - |
| Numero Protocollo | `text` |  | - |
| Data Protocollo | `ddm-date` |  | - |
| Data Pubblicazione | `ddm-date` |  | - |
| Data Fine Pubblicazione | `ddm-date` |  | - |
| Tipologia | `text` |  | - |
| Ente Proponente | `text` |  | - |
| Nome Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Allegato Esterno | `text` |  | - |
| Link | `text` |  | - |
| Nascondi dagli albi | `checkbox` |  | - |

