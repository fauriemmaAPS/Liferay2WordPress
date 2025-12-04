# PNRR_scuole - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `10154040`
**Structure ID:** `10154041`
**ClassNameId:** `10109` - CMN - Contenuti
**Parent Menu:** CMN - Contenuti

## Description

<?xml version='1.0' encoding='UTF-8'?><root available-locales="it_IT" default-locale="it_IT"><Description language-id="it_IT">Struttura per Mappa PNRR Scuole</Description></root>

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

This post type has **4** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Titolo del progetto | `textarea` |  | - |
| CUP | `text` |  | - |
| Misura PNRR | `textarea` |  | - |
| Importo | `ddm-number` |  | - |

