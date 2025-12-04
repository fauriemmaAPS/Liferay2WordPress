# Utente Metropolitano - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `28931`
**Structure ID:** `28932`
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

This post type has **26** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Nome Cognome | `text` |  | - |
| Ruolo | `select` |  | - |
| Non Definito | `option` |  | - |
| Sindaco | `option` |  | - |
| Vice Sindaco | `option` |  | - |
| Consigliere | `option` |  | - |
| Capogruppo | `option` |  | - |
| Indipendente | `option` |  | - |
| Consigliere Delegato | `option` |  | - |
| Commissario Prefettizio | `option` |  | - |
| Sindaco Facente Funzioni | `option` |  | - |
| Commissione Straordinaria | `option` |  | - |
|  | `option` |  | - |
| Sito Web | `text` |  | - |
| Email | `text` |  | - |
| Indirizzo | `text` |  | - |
| Foto | `wcm-image` |  | - |
| Descrizione | `ddm-text-html` |  | - |
| Curriculum | `ddm-documentlibrary` |  | - |
| Nome Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Allegato Nascosto | `checkbox` |  | - |
| Consiglio Metropolitano | `checkbox` |  | - |
| Conferenza Metropolitana | `checkbox` |  | - |
| Sindaco Metropolitano | `checkbox` |  | - |
| Archivio Consiglio Metropolitano | `checkbox` |  | - |

