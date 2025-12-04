# StrutturaConsulente - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `1261149`
**Structure ID:** `1261150`
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
| Estremi dell'atto di conferimento dell'incarico | `textarea` |  | - |
| Oggetto | `text` |  | - |
| Normativa di riferimento | `textarea` |  | - |
| Data inizio incarico | `ddm-date` |  | - |
| senza data inizio | `checkbox` |  | - |
| Data fine incarico | `ddm-date` |  | - |
| senza data fine | `checkbox` |  | - |
| Compenso Lordo (€) | `ddm-decimal` |  | - |
| Componenti variabili del compenso o legate alla valutazione di risultato (€) | `ddm-decimal` |  | - |
| Svolgimento di incarichi o titolarità di cariche in enti di diritto privato regolati o finanziati dalla P.A. o relativi allo svolgimento di attività professionali (€) | `ddm-decimal` |  | - |
| Data Pubblicazione | `ddm-date` |  | - |
| Data di Pubblicazione | `checkbox` |  | - |
| Proprietà | `text` |  | - |
| Testo | `text` |  | - |
| Attestazioni art.53, comma 14 d.lgs 165/2001 | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Collegamento alla Pagina | `ddm-link-to-page` |  | - |
| Collegamento Esterno | `text` |  | - |
| Soggetto - Link al Curriculum vitae | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Collegamento alla Pagina | `ddm-link-to-page` |  | - |
| Collegamento Esterno | `text` |  | - |
| Allegato | `text` |  | - |
| Allegato | `ddm-documentlibrary` |  | - |
| Collegamento alla Pagina | `ddm-link-to-page` |  | - |
| Collegamento Esterno | `text` |  | - |

