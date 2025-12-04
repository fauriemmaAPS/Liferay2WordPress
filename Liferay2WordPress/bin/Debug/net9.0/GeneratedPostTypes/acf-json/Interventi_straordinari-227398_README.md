# Interventi straordinari - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `227398`
**Structure ID:** `227399`
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

This post type has **9** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| di concessione | `ddm-text-html` |  | - |
| Nominativo beneficiario | `ddm-text-html` |  | - |
|  P.IVA. o C.F.  | `ddm-text-html` |  | - |
| Importo del vantaggio economico | `ddm-text-html` |  | - |
| Norma o titolo a base dell’attribuzione | `ddm-text-html` |  | - |
| Ufficio | `ddm-text-html` |  | - |
| Funzionario o Dirigente Responsabile del procedimento | `ddm-text-html` |  | - |
| Progetto selezionato | `ddm-text-html` |  | - |
| Modalità seguita per individuazione del beneficiario | `ddm-text-html` |  | - |

