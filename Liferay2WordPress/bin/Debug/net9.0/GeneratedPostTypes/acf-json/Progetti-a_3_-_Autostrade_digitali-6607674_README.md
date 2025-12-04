# Progetti-a.3 - Autostrade digitali - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `6607674`
**Structure ID:** `6607675`
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

This post type has **11** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Comune | `textarea` |  | - |
| Codice Progetto | `text` |  | - |
| Livello di progettazione | `textarea` |  | - |
| Titolo del progetto | `textarea` |  | - |
| Importo finanziato da CM | `ddm-decimal` |  | - |
| Importo totale intervento | `ddm-decimal` |  | - |
| Importo Cofinanziato | `ddm-decimal` |  | - |
| Importo Speso | `ddm-decimal` |  | - |
| Asse Piano Strategico | `text` |  | - |
| Azione Piano Strategico | `text` |  | - |
| Termine ultimazione previsto | `ddm-date` |  | - |

