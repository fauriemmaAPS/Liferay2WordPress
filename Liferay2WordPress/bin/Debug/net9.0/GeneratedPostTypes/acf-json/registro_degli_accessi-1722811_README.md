# registro degli accessi - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `1722811`
**Structure ID:** `1722812`
**ClassNameId:** `10098` - CMN - Liste
**Parent Menu:** CMN - Liste

## Description

<?xml version='1.0' encoding='UTF-8'?><root available-locales="it_IT" default-locale="it_IT"><Description language-id="it_IT">Registro degli accessi (accesso civico)</Description></root>

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

This post type has **14** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| id | `ddm-integer` | ✓ | - |
| tipo | `select` |  | - |
| accesso 241/90 | `option` |  | - |
| accesso civico | `option` |  | - |
| accesso civico generalizzato | `option` |  | - |
| oggetto | `textarea` |  | - |
| data della richiesta | `ddm-date` |  | - |
| Esito | `select` |  | - |
|  | `option` |  | - |
| accoglimento | `option` |  | - |
| diniego | `option` |  | - |
| differimento | `option` |  | - |
| limitazione | `option` |  | - |
| data della decisione | `ddm-date` |  | - |

