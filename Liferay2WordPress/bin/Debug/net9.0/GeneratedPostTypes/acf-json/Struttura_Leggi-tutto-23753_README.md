# Struttura Leggi-tutto - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `23753`
**Structure ID:** `23754`
**ClassNameId:** `10109` - CMN - Contenuti
**Parent Menu:** CMN - Contenuti

## Description

<?xml version='1.0' encoding='UTF-8'?><root available-locales="it_IT" default-locale="it_IT"><Description language-id="it_IT">Struttura estendibile: Sezione1, Titolo Sezione 2,Sezione 2,</Description></root>

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
| Prima sezione HTML | `ddm-text-html` |  | - |
| Titolo seconda sezione | `text` |  | - |
| Seconda sezione HTML | `ddm-text-html` |  | - |
| Leggi Tutto | `checkbox` |  | - |

