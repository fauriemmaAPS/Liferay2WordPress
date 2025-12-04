<?php
/**
 * Plugin Name: CMN Custom Post Types
 * Plugin URI: 
 * Description: Abilita le nuove tipologie di Post (Articoli) utili alla Città Metropolitana di Napoli.
 * Version: 1.0.0
 * Author: Auriemma Francesco
 * Author URI: https://apsnet.it
 * License: MIT
 * Text Domain: cmn-custom-post-types
 * Generation Mode: ACF_JSON
 */

if (!defined('ABSPATH')) exit;

// ACF JSON Mode: Post Types and Taxonomies are loaded from JSON

// Register parent menu for CMN - Contenuti
function register_parent_menu_cmn_web_content() {
    add_menu_page(
        __('CMN - Contenuti', 'cmn-custom-post-types'),
        __('CMN - Contenuti', 'cmn-custom-post-types'),
        'edit_posts',
        'cmn_web_content',
        '',
        'dashicons-media-document',
        25
    );
}
add_action('admin_menu', 'register_parent_menu_cmn_web_content');

// Register parent menu for CMN - Liste
function register_parent_menu_cmn_other() {
    add_menu_page(
        __('CMN - Liste', 'cmn-custom-post-types'),
        __('CMN - Liste', 'cmn-custom-post-types'),
        'edit_posts',
        'cmn_other',
        '',
        'dashicons-admin-generic',
        25
    );
}
add_action('admin_menu', 'register_parent_menu_cmn_other');

// ACF JSON save point
add_filter('acf/settings/save_json', function($path) {
    return plugin_dir_path(__FILE__) . 'acf-json';
});

// ACF JSON load point
add_filter('acf/settings/load_json', function($paths) {
    $paths[] = plugin_dir_path(__FILE__) . 'acf-json';
    return $paths;
});

// Flush rewrite rules on activation
register_activation_hook(__FILE__, function() {
    // Trigger init to register post types and taxonomies
    do_action('init');
    // Flush rewrite rules
    flush_rewrite_rules();
});

// Flush rewrite rules on deactivation
register_deactivation_hook(__FILE__, function() {
    flush_rewrite_rules();
});
