# To Do - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `TO DO`
**Structure ID:** `12506`
**ClassNameId:** `10098` - CMN - Liste
**Parent Menu:** CMN - Liste

## Description

<?xml version='1.0' encoding='UTF-8'?><root available-locales="en_US" default-locale="en_US"><Description language-id="en_US">To Do</Description></root>

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
| Assigned To | `text` |  | - |
| Attachment | `ddm-documentlibrary` |  | - |
| Comments | `textarea` |  | - |
| Description | `textarea` |  | - |
| End Date | `ddm-date` |  | - |
| % Complete | `ddm-integer` |  | - |
| Severity | `select` |  | - |
| Critical | `option` |  | - |
| Major | `option` |  | - |
| Minor | `option` |  | - |
| Trivial | `option` |  | - |
| Start Date | `ddm-date` |  | - |
| Status | `select` |  | - |
| Open | `option` |  | - |
| Pending | `option` |  | - |
| Completed | `option` |  | - |
| Title | `text` |  | - |

