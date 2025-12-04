# Liferay Migration Helper - WordPress Plugin

Plugin WordPress per facilitare la migrazione di contenuti da Liferay Portal, abilitando l'upload di tutti i tipi di file comuni e gestendo file senza estensione (UUID).

## ?? Caratteristiche

- ? **Supporto MIME types estesi** - PDF, Office, immagini, video, audio, archivi
- ? **File senza estensione** - Gestisce file Liferay con nomi UUID
- ? **Limiti upload aumentati** - Fino a 100MB
- ? **Bypass controlli sicurezza** - Solo per amministratori durante migrazione
- ? **Auto-detection MIME type** - Rileva tipo file dal contenuto
- ? **Logging debug** - Traccia upload in modalità debug

## ?? Installazione

### Metodo 1: Copia Manuale

1. Copia la cartella `liferay-migration-helper` in `wp-content/plugins/`
   ```sh
   cp -r WordPressPlugin/liferay-migration-helper /path/to/wordpress/wp-content/plugins/
   ```

2. Vai su WordPress Admin ? Plugin
3. Attiva "Liferay Migration Helper"

### Metodo 2: Via FTP/SFTP

1. Connettiti al server via FTP
2. Naviga in `wp-content/plugins/`
3. Carica la cartella `liferay-migration-helper`
4. Attiva il plugin dal pannello WordPress

## ?? Configurazione

### wp-config.php

Aggiungi queste righe **prima** di `/* That's all, stop editing! */`:

```php
// Aumenta memoria e limiti upload
define('WP_MEMORY_LIMIT', '256M');
define('WP_MAX_MEMORY_LIMIT', '512M');

@ini_set('upload_max_filesize', '100M');
@ini_set('post_max_size', '100M');
@ini_set('max_execution_time', '300');

// Abilita debug (opzionale, per troubleshooting)
define('WP_DEBUG', true);
define('WP_DEBUG_LOG', true);
define('WP_DEBUG_DISPLAY', false);
```

### .htaccess (Apache)

Se usi Apache, aggiungi al file `.htaccess`:

```apache
php_value upload_max_filesize 100M
php_value post_max_size 100M
php_value max_execution_time 300
php_value max_input_time 300
php_value memory_limit 256M
```

### nginx.conf (Nginx)

Se usi Nginx, aggiungi nella configurazione del server:

```nginx
client_max_body_size 100M;

location ~ \.php$ {
    fastcgi_param PHP_VALUE "upload_max_filesize=100M \n post_max_size=100M";
}
```

## ?? Utilizzo

### Durante la Migrazione

Il plugin è attivo automaticamente una volta installato. Non richiede configurazione aggiuntiva.

### Verifica Funzionamento

1. Vai su **Impostazioni ? Liferay Migration**
2. Verifica i tipi di file supportati
3. Testa l'upload manuale di un file

### Test REST API

Testa l'upload via REST API:

```sh
curl -X POST https://your-site.com/wp-json/wp/v2/media \
  -u "username:application_password" \
  -F "file=@test-file.jpg" \
  -F "title=Test Upload"
```

## ?? Tipi di File Supportati

| Categoria | Formati |
|-----------|---------|
| **Documenti** | PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX |
| **OpenDocument** | ODT, ODS, ODP |
| **Immagini** | JPG, PNG, GIF, BMP, TIFF, ICO, SVG, WEBP |
| **Archivi** | ZIP, RAR, 7Z, TAR, GZ |
| **Video** | MP4, MOV, MPEG, AVI, WMV, FLV, WEBM |
| **Audio** | MP3, WAV, OGG, FLAC, AAC |
| **Testo** | TXT, CSV, XML, JSON, HTML |
| **Firma Digitale** | P7S, P7M |
| **Email** | EML, MSG |
| **Binari** | File senza estensione, BIN |

## ?? Sicurezza

### Importante

?? **Questo plugin disabilita alcuni controlli di sicurezza di WordPress per permettere l'upload di qualsiasi file.**

- Funziona **solo per utenti Amministratori**
- **Disattiva il plugin** dopo aver completato la migrazione
- Non usare in produzione per lunghi periodi

### Raccomandazioni

1. Usa il plugin **solo durante la migrazione**
2. Mantieni WordPress aggiornato
3. Usa password forti per gli account amministratore
4. Abilita autenticazione a due fattori
5. Monitora i log di WordPress

## ?? Troubleshooting

### File Upload Fallisce

**Problema:** Errore 500 o "Non hai i permessi"

**Soluzione:**
1. Verifica che il plugin sia attivo
2. Controlla `wp-content/debug.log`
3. Aumenta limiti PHP in `wp-config.php`
4. Verifica permessi file su `wp-content/uploads/` (755)

### File Troppo Grande

**Problema:** "Il file supera i limiti"

**Soluzione:**
1. Aumenta `upload_max_filesize` e `post_max_size`
2. Riavvia PHP-FPM/Apache
3. Verifica limiti con `phpinfo()`

### MIME Type Non Riconosciuto

**Problema:** File rifiutato come "tipo non permesso"

**Soluzione:**
1. Il plugin usa `finfo` per rilevare tipo file
2. Se fallisce, il file è accettato come `application/octet-stream`
3. Controlla che estensione PHP `fileinfo` sia abilitata

### Log Debug

Controlla i log in `wp-content/debug.log`:

```sh
tail -f wp-content/debug.log
```

## ?? Monitoraggio

### Verificare Upload Riusciti

Nel database WordPress:

```sql
SELECT ID, post_title, post_mime_type, guid 
FROM wp_posts 
WHERE post_type = 'attachment' 
ORDER BY ID DESC 
LIMIT 20;
```

### Statistiche Upload

Nel pannello admin:
1. **Media ? Libreria** - Visualizza tutti i file caricati
2. **Impostazioni ? Liferay Migration** - Statistiche e log

## ?? Dopo la Migrazione

### Disattivazione

1. Vai su **Plugin**
2. Disattiva "Liferay Migration Helper"
3. (Opzionale) Elimina il plugin

### Ripristino Sicurezza

Una volta disattivato il plugin:
- I controlli MIME standard di WordPress vengono ripristinati
- Gli upload tornano ai limiti predefiniti
- I file già caricati rimangono disponibili

## ??? Sviluppo

### Struttura File

```
liferay-migration-helper/
??? liferay-migration-helper.php  # File principale plugin
??? README.md                      # Questa documentazione
```

### Hook Disponibili

Il plugin usa questi filtri WordPress:

- `upload_mimes` - Aggiunge MIME types
- `wp_check_filetype_and_ext` - Bypass controlli estensioni
- `wp_max_upload_size` - Aumenta limite upload
- `add_attachment` - Log upload (se debug abilitato)

### Personalizzazione

Per aggiungere nuovi MIME types, modifica la funzione:

```php
function liferay_migration_allow_all_mimes($mimes) {
    // Aggiungi il tuo MIME type
    $mimes['custom'] = 'application/x-custom';
    return $mimes;
}
```

## ?? Changelog

### Version 1.0.0 (2024)
- Rilascio iniziale
- Supporto MIME types estesi
- File senza estensione (UUID Liferay)
- Pannello amministrazione
- Logging debug

## ?? Licenza

MIT License - Usa liberamente per i tuoi progetti

## ?? Supporto

Per problemi o domande:
1. Controlla questa documentazione
2. Verifica i log WordPress (`wp-content/debug.log`)
3. Apri una issue su GitHub

## ?? Link Utili

- [WordPress REST API Media](https://developer.wordpress.org/rest-api/reference/media/)
- [WordPress Upload MIME Types](https://developer.wordpress.org/reference/functions/get_allowed_mime_types/)
- [PHP File Upload](https://www.php.net/manual/en/features.file-upload.php)
- [Liferay Document Library](https://learn.liferay.com/w/dxp/content-authoring-and-management/documents-and-media)

---

**?? Ricorda:** Disattiva questo plugin dopo aver completato la migrazione per ripristinare i controlli di sicurezza standard di WordPress.
