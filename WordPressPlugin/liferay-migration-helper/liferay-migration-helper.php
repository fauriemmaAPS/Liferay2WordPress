<?php
/**
 * Plugin Name: Liferay Migration Helper
 * Plugin URI: https://github.com/yourusername/liferay2wordpress
 * Description: Abilita upload di tutti i tipi di file durante la migrazione da Liferay. Include supporto per file senza estensione e MIME types estesi.
 * Version: 1.0.0
 * Author: Your Name
 * Author URI: https://yourwebsite.com
 * License: MIT
 * Text Domain: liferay-migration-helper
 */

if (!defined('ABSPATH')) {
    exit; // Exit if accessed directly
}

/**
 * Abilita tutti i tipi MIME comuni per la migrazione
 */
add_filter('upload_mimes', 'liferay_migration_allow_all_mimes', 999);
function liferay_migration_allow_all_mimes($mimes) {
    // Documenti Office
    $mimes['pdf']  = 'application/pdf';
    $mimes['doc']  = 'application/msword';
    $mimes['docx'] = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
    $mimes['xls']  = 'application/vnd.ms-excel';
    $mimes['xlsx'] = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    $mimes['ppt']  = 'application/vnd.ms-powerpoint';
    $mimes['pptx'] = 'application/vnd.openxmlformats-officedocument.presentationml.presentation';
    
    // OpenDocument
    $mimes['odt']  = 'application/vnd.oasis.opendocument.text';
    $mimes['ods']  = 'application/vnd.oasis.opendocument.spreadsheet';
    $mimes['odp']  = 'application/vnd.oasis.opendocument.presentation';
    
    // Immagini
    $mimes['jpg|jpeg|jpe'] = 'image/jpeg';
    $mimes['png']  = 'image/png';
    $mimes['gif']  = 'image/gif';
    $mimes['bmp']  = 'image/bmp';
    $mimes['tif|tiff'] = 'image/tiff';
    $mimes['ico']  = 'image/x-icon';
    $mimes['svg']  = 'image/svg+xml';
    $mimes['svgz'] = 'image/svg+xml';
    $mimes['webp'] = 'image/webp';
    
    // Archivi
    $mimes['zip'] = 'application/zip';
    $mimes['rar'] = 'application/x-rar-compressed';
    $mimes['7z']  = 'application/x-7z-compressed';
    $mimes['tar'] = 'application/x-tar';
    $mimes['gz|gzip'] = 'application/x-gzip';
    
    // Video
    $mimes['mp4|m4v']     = 'video/mp4';
    $mimes['mov|qt']      = 'video/quicktime';
    $mimes['mpeg|mpg|mpe'] = 'video/mpeg';
    $mimes['avi']         = 'video/x-msvideo';
    $mimes['wmv']         = 'video/x-ms-wmv';
    $mimes['flv']         = 'video/x-flv';
    $mimes['webm']        = 'video/webm';
    
    // Audio
    $mimes['mp3|m4a|m4b'] = 'audio/mpeg';
    $mimes['wav']         = 'audio/wav';
    $mimes['ogg|oga']     = 'audio/ogg';
    $mimes['flac']        = 'audio/flac';
    $mimes['aac']         = 'audio/aac';
    
    // Testo
    $mimes['txt|asc|c|cc|h'] = 'text/plain';
    $mimes['csv'] = 'text/csv';
    $mimes['xml'] = 'application/xml';
    $mimes['json'] = 'application/json';
    $mimes['htm|html'] = 'text/html';
    
    // File senza estensione o binari (Liferay UUID files)
    $mimes['bin'] = 'application/octet-stream';
    
    // Firma digitale
    $mimes['p7s'] = 'application/pkcs7-signature';
    $mimes['p7m'] = 'application/pkcs7-mime';
    
    // Email
    $mimes['eml'] = 'message/rfc822';
    $mimes['msg'] = 'application/vnd.ms-outlook';
    
    return $mimes;
}

/**
 * Disabilita controllo sicurezza estensioni file per amministratori
 * Permette upload di file senza estensione (come UUID di Liferay)
 */
add_filter('wp_check_filetype_and_ext', 'liferay_migration_bypass_filetype_check', 10, 4);
function liferay_migration_bypass_filetype_check($data, $file, $filename, $mimes) {
    // Solo per amministratori
    if (!current_user_can('administrator')) {
        return $data;
    }
    
    // Se non ha estensione o non è riconosciuto, usa generico
    if (empty($data['ext']) && empty($data['type'])) {
        // Prova a rilevare dal contenuto
        $finfo = finfo_open(FILEINFO_MIME_TYPE);
        $mime = finfo_file($finfo, $file);
        finfo_close($finfo);
        
        if ($mime && $mime !== 'application/octet-stream') {
            $data['type'] = $mime;
            $data['ext'] = liferay_get_extension_from_mime($mime);
        } else {
            // Fallback: accetta come binario
            $data['ext']  = 'bin';
            $data['type'] = 'application/octet-stream';
        }
        
        $data['proper_filename'] = $filename;
    }
    
    return $data;
}

/**
 * Ottiene estensione da MIME type
 */
function liferay_get_extension_from_mime($mime) {
    $mime_to_ext = array(
        'image/jpeg' => 'jpg',
        'image/png' => 'png',
        'image/gif' => 'gif',
        'image/bmp' => 'bmp',
        'image/webp' => 'webp',
        'image/svg+xml' => 'svg',
        'image/tiff' => 'tiff',
        'application/pdf' => 'pdf',
        'application/zip' => 'zip',
        'application/msword' => 'doc',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document' => 'docx',
        'application/vnd.ms-excel' => 'xls',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' => 'xlsx',
        'application/vnd.ms-powerpoint' => 'ppt',
        'application/vnd.openxmlformats-officedocument.presentationml.presentation' => 'pptx',
        'text/plain' => 'txt',
        'text/html' => 'html',
        'text/csv' => 'csv',
        'application/json' => 'json',
        'application/xml' => 'xml',
        'video/mp4' => 'mp4',
        'audio/mpeg' => 'mp3',
    );
    
    return isset($mime_to_ext[$mime]) ? $mime_to_ext[$mime] : 'bin';
}

/**
 * Aumenta limiti upload
 */
add_filter('wp_max_upload_size', 'liferay_migration_increase_upload_limit');
function liferay_migration_increase_upload_limit($size) {
    return 1024 * 1024 * 100; // 100MB
}

/**
 * Permetti upload anche per file grandi
 */
add_filter('upload_size_limit', 'liferay_migration_increase_upload_limit');

/**
 * Log upload per debug (solo in modalità debug)
 */
if (defined('WP_DEBUG') && WP_DEBUG) {
    add_action('add_attachment', 'liferay_migration_log_upload');
    function liferay_migration_log_upload($attachment_id) {
        $file = get_attached_file($attachment_id);
        $mime = get_post_mime_type($attachment_id);
        error_log("Liferay Migration: Uploaded file ID $attachment_id: $file (MIME: $mime)");
    }
}

/**
 * Aggiungi menu admin per gestione plugin
 */
add_action('admin_menu', 'liferay_migration_admin_menu');
function liferay_migration_admin_menu() {
    add_options_page(
        'Liferay Migration Helper',
        'Liferay Migration',
        'manage_options',
        'liferay-migration-helper',
        'liferay_migration_settings_page'
    );
}

/**
 * Pagina impostazioni
 */
function liferay_migration_settings_page() {
    ?>
    <div class="wrap">
        <h1>Liferay Migration Helper</h1>
        
        <div class="notice notice-info">
            <p><strong>Plugin attivo!</strong> Le seguenti funzionalità sono abilitate:</p>
            <ul style="list-style: disc; margin-left: 20px;">
                <li>Upload di tutti i tipi di file MIME comuni</li>
                <li>Supporto file senza estensione (UUID Liferay)</li>
                <li>Limite upload aumentato a 100MB</li>
                <li>Bypass controlli sicurezza per amministratori</li>
            </ul>
        </div>
        
        <h2>Tipi di File Supportati</h2>
        <table class="widefat striped">
            <thead>
                <tr>
                    <th>Categoria</th>
                    <th>Formati</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><strong>Documenti</strong></td>
                    <td>PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, ODT, ODS, ODP</td>
                </tr>
                <tr>
                    <td><strong>Immagini</strong></td>
                    <td>JPG, PNG, GIF, BMP, TIFF, ICO, SVG, WEBP</td>
                </tr>
                <tr>
                    <td><strong>Archivi</strong></td>
                    <td>ZIP, RAR, 7Z, TAR, GZ</td>
                </tr>
                <tr>
                    <td><strong>Video</strong></td>
                    <td>MP4, MOV, MPEG, AVI, WMV, FLV, WEBM</td>
                </tr>
                <tr>
                    <td><strong>Audio</strong></td>
                    <td>MP3, WAV, OGG, FLAC, AAC</td>
                </tr>
                <tr>
                    <td><strong>Testo</strong></td>
                    <td>TXT, CSV, XML, JSON, HTML</td>
                </tr>
                <tr>
                    <td><strong>Altri</strong></td>
                    <td>File binari, P7S, P7M, EML, MSG</td>
                </tr>
            </tbody>
        </table>
        
        <h2>Configurazione PHP Consigliata</h2>
        <p>Aggiungi al file <code>wp-config.php</code>:</p>
        <pre style="background: #f5f5f5; padding: 10px; border: 1px solid #ddd;">
define('WP_MEMORY_LIMIT', '256M');
define('WP_MAX_MEMORY_LIMIT', '512M');

@ini_set('upload_max_filesize', '100M');
@ini_set('post_max_size', '100M');
@ini_set('max_execution_time', '300');
        </pre>
        
        <h2>Test Upload</h2>
        <p>Prova l'upload tramite REST API:</p>
        <pre style="background: #f5f5f5; padding: 10px; border: 1px solid #ddd;">
curl -X POST <?php echo get_rest_url(null, 'wp/v2/media'); ?> \
  -u "username:application_password" \
  -F "file=@test.jpg"
        </pre>
        
        <h2>Disattivazione</h2>
        <p>
            <strong>Importante:</strong> Disattiva questo plugin dopo aver completato la migrazione 
            per ripristinare i controlli di sicurezza standard di WordPress.
        </p>
    </div>
    <?php
}

/**
 * Avviso di sicurezza nell'admin
 */
add_action('admin_notices', 'liferay_migration_security_notice');
function liferay_migration_security_notice() {
    $screen = get_current_screen();
    if ($screen->id !== 'plugins') {
        return;
    }
    ?>
    <div class="notice notice-warning">
        <p>
            <strong>Liferay Migration Helper:</strong> 
            Questo plugin disabilita alcuni controlli di sicurezza per permettere la migrazione. 
            <a href="<?php echo admin_url('options-general.php?page=liferay-migration-helper'); ?>">
                Visualizza dettagli
            </a>
        </p>
    </div>
    <?php
}
