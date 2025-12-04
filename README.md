# Liferay2WordPress

Strumenti per migrare contenuti da Liferay a WordPress con .NET 9.

Funzionalità principali
- Conversione articoli Liferay in post/pagine WordPress
- Migrazione media (documenti, immagini) con ricollegamento
- Mappatura categorie e tag
- Conversione template Freemarker/Velocity in PHP per WordPress
- Generazione Custom Post Types e Taxonomies via ACF JSON o PHP
- Ripresa migrazione grazie allo stato persistito

Struttura progetto
- `Data`: repository per articoli, template, utenti, media (Liferay)
- `Services`: logica di migrazione, conversione template, client WordPress
- `Models`: modelli di supporto
- `Migrator`: orchestratore della migrazione
- `Program`: entrypoint CLI

Prerequisiti
- .NET 9 SDK
- Accesso al DB Liferay (MySQL/MariaDB)
- WordPress con REST API attive
- (Opzionale) ACF PRO per gestione CPT via JSON

Utilizzo
1. Configura credenziali e opzioni in `appsettings.json`
2. Esegui da CLI: `dotnet run`
3. Scegli tra migrazione completa o sola generazione template/CPT

Note operative
- Dopo generazione CPT/tassonomie: aggiornare permalink in WordPress
- I template PHP generati vanno copiati nel tema attivo
- In modalità ACF JSON, copiare la cartella `acf-json/` nel tema o plugin

Supporto
- Problemi di build: `dotnet restore && dotnet build`
- Verifica `wp-json` e Application Password per WordPress

Licenza
- MIT
