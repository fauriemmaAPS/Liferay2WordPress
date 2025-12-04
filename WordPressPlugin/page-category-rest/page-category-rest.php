<?php
/*
Plugin Name: Page Category (REST)
Description: Registers a custom hierarchical taxonomy for pages with REST support.
Version: 1.0.0
Author: Migration Tool
*/

if (!defined('ABSPATH')) { exit; }

function pc_register_page_category_taxonomy() {
    $labels = array(
        'name'              => _x('Page Categories', 'taxonomy general name'),
        'singular_name'     => _x('Page Category', 'taxonomy singular name'),
        'search_items'      => __('Search Page Categories'),
        'all_items'         => __('All Page Categories'),
        'parent_item'       => __('Parent Page Category'),
        'parent_item_colon' => __('Parent Page Category:'),
        'edit_item'         => __('Edit Page Category'),
        'update_item'       => __('Update Page Category'),
        'add_new_item'      => __('Add New Page Category'),
        'new_item_name'     => __('New Page Category Name'),
        'menu_name'         => __('Page Categories'),
    );

    register_taxonomy('page_category', 'page', array(
        'hierarchical'      => true,
        'labels'            => $labels,
        'show_ui'           => true,
        'show_admin_column' => true,
        'query_var'         => true,
        'rewrite'           => array('slug' => 'page-category'),
        'show_in_rest'      => true,
        'rest_base'         => 'page_category',
        'rest_controller_class' => 'WP_REST_Terms_Controller',
    ));
}
add_action('init', 'pc_register_page_category_taxonomy');

