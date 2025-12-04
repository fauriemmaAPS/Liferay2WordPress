# Incarichi amministrativi di vertice - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `137899`
**Structure ID:** `137900`
**ClassNameId:** `10098` - CMN - Liste
**Parent Menu:** CMN - Liste

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

**Note:** This post type will appear under the **CMN - Liste** menu in WordPress admin.

### 4. Flush Rewrite Rules

Go to **WordPress Admin → Settings → Permalinks** and click "Save Changes"

## Custom Fields

This post type has **7** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Nominativo e incarico ricoperto | `ddm-text-html` |  | - |
| Curriculum | `ddm-text-html` |  | - |
| Retribuzione | `ddm-text-html` |  | - |
| Atto di nomina | `ddm-text-html` |  | - |
| Dichiarazione sulle cause di incompatibilità ed inconferibilità | `ddm-text-html` |  | - |
| Situazione patrimoniale 2015 | `ddm-text-html` |  | - |
| Collegamento alla Pagina | `ddm-link-to-page` |  | - |

