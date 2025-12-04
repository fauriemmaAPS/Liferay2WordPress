# Enti di diritto privato controllati - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `158027`
**Structure ID:** `158028`
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

This post type has **5** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Enti di diritto privato controllati | `ddm-text-html` |  | - |
| Scheda | `ddm-text-html` |  | - |
| Bilancio | `ddm-text-html` |  | - |
| Statuto | `ddm-text-html` |  | - |
| ULTIMO AGGIORNAMENTO | `ddm-date` |  | - |

