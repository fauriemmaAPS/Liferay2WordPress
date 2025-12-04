# Interventi straordinari e di emergenza ai sensi dell\'art.42 - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `139310`
**Structure ID:** `139311`
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

This post type has **17** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| di concessione | `ddm-documentlibrary` |  | - |
| Nominativo beneficiario | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| P.IVA. o C.F. | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Importo del vantaggio economico | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Norma o titolo a base dell'attribuzione | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Ufficio | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Funzionario o Dirigente Responsabile del procedimento | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Modalità seguita per individuazione del beneficiario | `text` |  | - |
| HTML | `ddm-text-html` |  | - |
| Progetto selezionato | `text` |  | - |
| HTML | `ddm-text-html` |  | - |

