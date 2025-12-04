# News - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `12714`
**Structure ID:** `12715`
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
| Abstract | `text` |  | - |
| Link Esterno | `text` |  | - |
| Contenuto News | `ddm-text-html` |  | - |
| Data Evento | `text` |  | - |
| Nome Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Immagine di Primo Piano (880x300) | `wcm-image` |  | - |
| Immagine logo (90x90) | `wcm-image` |  | - |
| Visualizza in: | `radio` |  | - |
| Solo nel canale | `option` |  | - |
| Primo Piano | `option` |  | - |
| News di Sinistra | `option` |  | - |
| News di Destra | `option` |  | - |

