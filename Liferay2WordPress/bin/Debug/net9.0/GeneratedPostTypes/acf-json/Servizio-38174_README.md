# Servizio - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `38174`
**Structure ID:** `38175`
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

This post type has **20** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Avviso | `ddm-text-html` |  | - |
| Informazioni | `ddm-text-html` |  | - |
| Orari apertura al pubblico | `ddm-text-html` |  | - |
| Chi può effettuare la richiesta | `ddm-text-html` |  | - |
| Modalità di presentazione | `ddm-text-html` |  | - |
| Prescrizioni | `ddm-text-html` |  | - |
| Termini per la presentazione | `ddm-text-html` |  | - |
| Documenti da consegnare | `ddm-text-html` |  | - |
| Modalità di rilascio | `ddm-text-html` |  | - |
| Ritiro | `ddm-text-html` |  | - |
| Validità | `ddm-text-html` |  | - |
| Eventuali costi | `ddm-text-html` |  | - |
| Normativa di riferimento | `ddm-text-html` |  | - |
| Proprietà | `text` |  | - |
| Testo | `ddm-text-html` |  | - |
| Collegameto a pagina | `checkbox` |  | - |
| Testo da visualizzare | `text` |  | - |
| URL (Es. "http://www.google.com") | `text` |  | - |
| Nome Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |

