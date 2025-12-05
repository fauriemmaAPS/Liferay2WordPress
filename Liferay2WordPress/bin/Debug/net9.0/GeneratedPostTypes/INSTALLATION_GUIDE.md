# WordPress Custom Post Types Installation Guide

**Generation Mode:** `ACF_JSON`
**Generated on:** 2025-12-05 11:02:39
**Total structures:** 99

## 🎯 ACF JSON Mode

This generation uses **ACF PRO 6.1+ JSON format** for:
- ✅ Custom Post Types (`post-type-*.json`)
- ✅ Taxonomies (`taxonomy-*.json`)
- ✅ Custom Fields (`group-*.json`)

**Advantages:**
- No PHP code for post types and taxonomies
- Visual management via ACF UI
- Easy sync between environments
- Version control friendly

## Quick Installation

1. **Install ACF PRO** - Version 6.1+ required for ACF JSON mode
2. **Copy ACF JSON** - Copy all files from `acf-json/` to your theme's `acf-json/` folder
3. **Install Plugin** - Upload and activate `cmn-custom-post-types.php` (only for parent menus)
   - ACF will automatically register post types and taxonomies from JSON
4. **Flush Permalinks** - Go to Settings → Permalinks → Save

## Parent Menus

Custom Post Types are organized under parent menus based on their Liferay classNameId:

### CMN - Contenuti (ClassNameId: 10109)
- **Menu Slug:** `cmn_web_content`
- **Icon:** `dashicons-media-document`
- **Post Types Count:** 58

  - **Agenzia Stampa** (`45411`) - 3 fields
  - **Albo Pretorio** (`17193`) - 13 fields
  - **Albo Pretorio Scaduto** (`604250`) - 13 fields
  - **Area Funzionale 01** (`23891`) - 13 fields
  - **Bando di concorso** (`45037`) - 11 fields
  - **Bando di gara** (`1017697`) - 24 fields
  - **Bando di gara trasp** (`1012239`) - 21 fields
  - **Bando di garan** (`29731`) - 11 fields
  - **CARD_AGID** (`14714559`) - 3 fields
  - **CardBanner** (`5563290`) - 1 fields
  - **Comune** (`23611`) - 16 fields
  - **Comunicati** (`28641`) - 3 fields
  - **Comunicati stampa** (`12765579`) - 2 fields
  - **Contenuto Generale** (`37395`) - 12 fields
  - **Coordinate Progetti** (`5659945`) - 1 fields
  - **Defibrillatori** (`5668314`) - 5 fields
  - **dirigenti (foia)** (`1656527`) - 10 fields
  - **Elenco Partecipate** (`975443`) - 3 fields
  - **Email_Istituzionali** (`1971713`) - 4 fields
  - **Menu a Tendina (generico)** (`38592`) - 3 fields
  - **Modulo Generale** (`34496`) - 12 fields
  - **Modulo Generale copia** (`3438606`) - 10 fields
  - **Modulo Generale2** (`3433649`) - 12 fields
  - **monitoraggio_procedimenti** (`1005793`) - 53 fields
  - **News** (`12714`) - 13 fields
  - **News_Fix** (`2712079`) - 8 fields
  - **nuovo rdf** (`3680950`) - 11 fields
  - **Offerte di lavoro** (`38925`) - 10 fields
  - **opere** (`2247234`) - 5 fields
  - **Piano Strategico** (`3497805`) - 12 fields
  - **Piano Strategico Collapsible** (`3499767`) - 14 fields
  - **PNRR_scuole** (`10154040`) - 4 fields
  - **PNRR_scuole_sismico** (`10220251`) - 4 fields
  - **progetti comuni** (`4325270`) - 13 fields
  - **Progetti Comuni** (`5129177`) - 11 fields
  - **Progetti-a.1 - Cultura come sviluppo** (`6586964`) - 11 fields
  - **Progetti-a.2 - Scuole presidio di legalità ed integrazione** (`6607672`) - 11 fields
  - **Progetti-a.3 - Autostrade digitali** (`6607674`) - 11 fields
  - **Progetti-b.1 - Consumo di suolo Zero** (`6607676`) - 11 fields
  - **Progetti-b.2 - Ossigeno Bene Comune** (`6607678`) - 11 fields
  - **Progetti-b.3 - Città sicure** (`6607680`) - 11 fields
  - **Provvedimenti Scaduti** (`3108817`) - 10 fields
  - **Provvedimento Dirigente** (`211617`) - 10 fields
  - **rdf** (`3660056`) - 11 fields
  - **Servizio** (`38174`) - 20 fields
  - **Servizio Viabilità** (`1654308`) - 11 fields
  - **Strutt2** (`8014255`) - 0 fields
  - **struttura determine a contrarre** (`1732111`) - 10 fields
  - **Struttura Leggi-tutto** (`23753`) - 4 fields
  - **struttura procedimenti** (`979058`) - 105 fields
  - **StrutturaArt27** (`1074220`) - 30 fields
  - **StrutturaConsiglioMetropolitano** (`1243107`) - 41 fields
  - **StrutturaConsulente** (`1261149`) - 26 fields
  - **Titolo Progetto** (`5563439`) - 1 fields
  - **URP** (`23939`) - 3 fields
  - **URP_CARD_AGID** (`5563349`) - 3 fields
  - **Utente Metropolitano** (`28931`) - 26 fields
  - **WebTv** (`1480743`) - 7 fields

### CMN - Liste (ClassNameId: 10098)
- **Menu Slug:** `cmn_other`
- **Icon:** `dashicons-admin-generic`
- **Post Types Count:** 41

  - **Attivazione** (`79755`) - 2 fields
  - **Attività** (`12601`) - 3 fields
  - **Attività AGID** (`5563458`) - 3 fields
  - **Autorizzazione Unica Ambientale** (`76360`) - 3 fields
  - **Bandi di gare e contratti ai sensi dell\'art.37 del d.lgs.33/2013** (`156607`) - 4 fields
  - **Componenti del nucleo di Valutazione per l\'anno 2014** (`140355`) - 4 fields
  - **Componenti del Nucleo di Valutazione per l\'anno 2014** (`1399203`) - 4 fields
  - **Contacts** (`contacts`) - 13 fields
  - **Dati Tabella** (`31819`) - 6 fields
  - **Dichiarazioni sulla insussistenza** (`141816`) - 5 fields
  - **Elenco certificazioni di avvenuta bonifica** (`76954`) - 2 fields
  - **Enti di diritto privato controllati** (`158027`) - 5 fields
  - **Enti pubblici vigilati** (`154245`) - 5 fields
  - **Events** (`events`) - 7 fields
  - **Graduatorie** (`77350`) - 3 fields
  - **hamef** (`109933`) - 2 fields
  - **Incarichi amministrativi di vertice** (`137899`) - 7 fields
  - **Interventi straordinari** (`227398`) - 9 fields
  - **Interventi straordinari e di emergenza ai sensi dell\'art.42** (`139310`) - 17 fields
  - **Inventory** (`inventory`) - 6 fields
  - **Ipotesi A** (`79850`) - 2 fields
  - **Ipotesi B :** (`79895`) - 2 fields
  - **Ipotesi B1** (`79917`) - 2 fields
  - **Ipotesi B2** (`79927`) - 2 fields
  - **Issues Tracking** (`issues_tracking`) - 16 fields
  - **Lista menu destra** (`5563344`) - 2 fields
  - **Meeting Minutes** (`meeting_minutes`) - 7 fields
  - **Menù** (`8794282`) - 2 fields
  - **modello lista prova** (`819356`) - 8 fields
  - **Pubblicazioni** (`12619`) - 4 fields
  - **registro degli accessi** (`1722811`) - 14 fields
  - **Rete museale metropolitana** (`80249`) - 5 fields
  - **Revisori dei conti** (`1970671`) - 5 fields
  - **Servizi** (`12588`) - 3 fields
  - **Siti Tematici** (`12569`) - 3 fields
  - **Sito Tematico** (`5671899`) - 2 fields
  - **Società partecipate** (`158211`) - 5 fields
  - **Suddivisione territoriale per competenza delle Procure della Repubblica** (`93059`) - 3 fields
  - **TEST_DEF** (`8071192`) - 2 fields
  - **TipologiaProva** (`1338406`) - 1 fields
  - **To Do** (`to_do`) - 17 fields

## All Post Types

### Agenzia Stampa
- **Slug:** `45411`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Albo Pretorio
- **Slug:** `17193`
- **Fields:** 13
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Albo Pretorio Scaduto
- **Slug:** `604250`
- **Fields:** 13
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Area Funzionale 01
- **Slug:** `23891`
- **Fields:** 13
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Attivazione
- **Slug:** `79755`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Attività
- **Slug:** `12601`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Attività AGID
- **Slug:** `5563458`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Autorizzazione Unica Ambientale
- **Slug:** `76360`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Bandi di gare e contratti ai sensi dell\'art.37 del d.lgs.33/2013
- **Slug:** `156607`
- **Fields:** 4
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Bando di concorso
- **Slug:** `45037`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Bando di gara
- **Slug:** `1017697`
- **Fields:** 24
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Bando di gara trasp
- **Slug:** `1012239`
- **Fields:** 21
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Bando di garan
- **Slug:** `29731`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### CARD_AGID
- **Slug:** `14714559`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### CardBanner
- **Slug:** `5563290`
- **Fields:** 1
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Componenti del nucleo di Valutazione per l\'anno 2014
- **Slug:** `140355`
- **Fields:** 4
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Componenti del Nucleo di Valutazione per l\'anno 2014
- **Slug:** `1399203`
- **Fields:** 4
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Comune
- **Slug:** `23611`
- **Fields:** 16
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Comunicati
- **Slug:** `28641`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Comunicati stampa
- **Slug:** `12765579`
- **Fields:** 2
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Contacts
- **Slug:** `contacts`
- **Fields:** 13
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Contenuto Generale
- **Slug:** `37395`
- **Fields:** 12
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Coordinate Progetti
- **Slug:** `5659945`
- **Fields:** 1
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Dati Tabella
- **Slug:** `31819`
- **Fields:** 6
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Defibrillatori
- **Slug:** `5668314`
- **Fields:** 5
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Dichiarazioni sulla insussistenza
- **Slug:** `141816`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### dirigenti (foia)
- **Slug:** `1656527`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Elenco certificazioni di avvenuta bonifica
- **Slug:** `76954`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Elenco Partecipate
- **Slug:** `975443`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Email_Istituzionali
- **Slug:** `1971713`
- **Fields:** 4
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Enti di diritto privato controllati
- **Slug:** `158027`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Enti pubblici vigilati
- **Slug:** `154245`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Events
- **Slug:** `events`
- **Fields:** 7
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Graduatorie
- **Slug:** `77350`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### hamef
- **Slug:** `109933`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Incarichi amministrativi di vertice
- **Slug:** `137899`
- **Fields:** 7
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Interventi straordinari
- **Slug:** `227398`
- **Fields:** 9
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Interventi straordinari e di emergenza ai sensi dell\'art.42
- **Slug:** `139310`
- **Fields:** 17
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Inventory
- **Slug:** `inventory`
- **Fields:** 6
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Ipotesi A
- **Slug:** `79850`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Ipotesi B :
- **Slug:** `79895`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Ipotesi B1
- **Slug:** `79917`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Ipotesi B2
- **Slug:** `79927`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Issues Tracking
- **Slug:** `issues_tracking`
- **Fields:** 16
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Lista menu destra
- **Slug:** `5563344`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Meeting Minutes
- **Slug:** `meeting_minutes`
- **Fields:** 7
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Menù
- **Slug:** `8794282`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Menu a Tendina (generico)
- **Slug:** `38592`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### modello lista prova
- **Slug:** `819356`
- **Fields:** 8
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Modulo Generale
- **Slug:** `34496`
- **Fields:** 12
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Modulo Generale copia
- **Slug:** `3438606`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Modulo Generale2
- **Slug:** `3433649`
- **Fields:** 12
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### monitoraggio_procedimenti
- **Slug:** `1005793`
- **Fields:** 53
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### News
- **Slug:** `12714`
- **Fields:** 13
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### News_Fix
- **Slug:** `2712079`
- **Fields:** 8
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### nuovo rdf
- **Slug:** `3680950`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Offerte di lavoro
- **Slug:** `38925`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### opere
- **Slug:** `2247234`
- **Fields:** 5
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Piano Strategico
- **Slug:** `3497805`
- **Fields:** 12
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Piano Strategico Collapsible
- **Slug:** `3499767`
- **Fields:** 14
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### PNRR_scuole
- **Slug:** `10154040`
- **Fields:** 4
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### PNRR_scuole_sismico
- **Slug:** `10220251`
- **Fields:** 4
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### progetti comuni
- **Slug:** `4325270`
- **Fields:** 13
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti Comuni
- **Slug:** `5129177`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-a.1 - Cultura come sviluppo
- **Slug:** `6586964`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-a.2 - Scuole presidio di legalità ed integrazione
- **Slug:** `6607672`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-a.3 - Autostrade digitali
- **Slug:** `6607674`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-b.1 - Consumo di suolo Zero
- **Slug:** `6607676`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-b.2 - Ossigeno Bene Comune
- **Slug:** `6607678`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Progetti-b.3 - Città sicure
- **Slug:** `6607680`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Provvedimenti Scaduti
- **Slug:** `3108817`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Provvedimento Dirigente
- **Slug:** `211617`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Pubblicazioni
- **Slug:** `12619`
- **Fields:** 4
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### rdf
- **Slug:** `3660056`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### registro degli accessi
- **Slug:** `1722811`
- **Fields:** 14
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Rete museale metropolitana
- **Slug:** `80249`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Revisori dei conti
- **Slug:** `1970671`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Servizi
- **Slug:** `12588`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Servizio
- **Slug:** `38174`
- **Fields:** 20
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Servizio Viabilità
- **Slug:** `1654308`
- **Fields:** 11
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Siti Tematici
- **Slug:** `12569`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Sito Tematico
- **Slug:** `5671899`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Società partecipate
- **Slug:** `158211`
- **Fields:** 5
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Strutt2
- **Slug:** `8014255`
- **Fields:** 0
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### struttura determine a contrarre
- **Slug:** `1732111`
- **Fields:** 10
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Struttura Leggi-tutto
- **Slug:** `23753`
- **Fields:** 4
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### struttura procedimenti
- **Slug:** `979058`
- **Fields:** 105
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### StrutturaArt27
- **Slug:** `1074220`
- **Fields:** 30
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### StrutturaConsiglioMetropolitano
- **Slug:** `1243107`
- **Fields:** 41
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### StrutturaConsulente
- **Slug:** `1261149`
- **Fields:** 26
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Suddivisione territoriale per competenza delle Procure della Repubblica
- **Slug:** `93059`
- **Fields:** 3
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### TEST_DEF
- **Slug:** `8071192`
- **Fields:** 2
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### TipologiaProva
- **Slug:** `1338406`
- **Fields:** 1
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### Titolo Progetto
- **Slug:** `5563439`
- **Fields:** 1
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### To Do
- **Slug:** `to_do`
- **Fields:** 17
- **Parent Menu:** CMN - Liste
- **ClassNameId:** 10098

### URP
- **Slug:** `23939`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### URP_CARD_AGID
- **Slug:** `5563349`
- **Fields:** 3
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### Utente Metropolitano
- **Slug:** `28931`
- **Fields:** 26
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

### WebTv
- **Slug:** `1480743`
- **Fields:** 7
- **Parent Menu:** CMN - Contenuti
- **ClassNameId:** 10109

## appsettings.json - CptMap

Inserisci la mappa seguente in `WordPress:CptMap`:

"WordPress": {
  "CptMap": {
    "45411": "post_type_45411",
    "17193": "post_type_17193",
    "604250": "post_type_604250",
    "23891": "post_type_23891",
    "79755": "post_type_79755",
    "12601": "post_type_12601",
    "5563458": "post_type_5563458",
    "76360": "post_type_76360",
    "156607": "post_type_156607",
    "45037": "post_type_45037",
    "1017697": "post_type_1017697",
    "1012239": "post_type_1012239",
    "29731": "post_type_29731",
    "14714559": "post_type_14714559",
    "5563290": "post_type_5563290",
    "140355": "post_type_140355",
    "1399203": "post_type_1399203",
    "23611": "post_type_23611",
    "28641": "post_type_28641",
    "12765579": "post_type_12765579",
    "CONTACTS": "post_type_contacts",
    "37395": "post_type_37395",
    "5659945": "post_type_5659945",
    "31819": "post_type_31819",
    "5668314": "post_type_5668314",
    "141816": "post_type_141816",
    "1656527": "post_type_1656527",
    "76954": "post_type_76954",
    "975443": "post_type_975443",
    "1971713": "post_type_1971713",
    "158027": "post_type_158027",
    "154245": "post_type_154245",
    "EVENTS": "post_type_events",
    "77350": "post_type_77350",
    "109933": "post_type_109933",
    "137899": "post_type_137899",
    "227398": "post_type_227398",
    "139310": "post_type_139310",
    "INVENTORY": "post_type_inventory",
    "79850": "post_type_79850",
    "79895": "post_type_79895",
    "79917": "post_type_79917",
    "79927": "post_type_79927",
    "ISSUES TRACKING": "post_type_issues_tracking",
    "5563344": "post_type_5563344",
    "MEETING MINUTES": "post_type_meeting_minutes",
    "8794282": "post_type_8794282",
    "38592": "post_type_38592",
    "819356": "post_type_819356",
    "34496": "post_type_34496",
    "3438606": "post_type_3438606",
    "3433649": "post_type_3433649",
    "1005793": "post_type_1005793",
    "12714": "post_type_12714",
    "2712079": "post_type_2712079",
    "3680950": "post_type_3680950",
    "38925": "post_type_38925",
    "2247234": "post_type_2247234",
    "3497805": "post_type_3497805",
    "3499767": "post_type_3499767",
    "10154040": "post_type_10154040",
    "10220251": "post_type_10220251",
    "4325270": "post_type_4325270",
    "5129177": "post_type_5129177",
    "6586964": "post_type_6586964",
    "6607672": "post_type_6607672",
    "6607674": "post_type_6607674",
    "6607676": "post_type_6607676",
    "6607678": "post_type_6607678",
    "6607680": "post_type_6607680",
    "3108817": "post_type_3108817",
    "211617": "post_type_211617",
    "12619": "post_type_12619",
    "3660056": "post_type_3660056",
    "1722811": "post_type_1722811",
    "80249": "post_type_80249",
    "1970671": "post_type_1970671",
    "12588": "post_type_12588",
    "38174": "post_type_38174",
    "1654308": "post_type_1654308",
    "12569": "post_type_12569",
    "5671899": "post_type_5671899",
    "158211": "post_type_158211",
    "8014255": "post_type_8014255",
    "1732111": "post_type_1732111",
    "23753": "post_type_23753",
    "979058": "post_type_979058",
    "1074220": "post_type_1074220",
    "1243107": "post_type_1243107",
    "1261149": "post_type_1261149",
    "93059": "post_type_93059",
    "8071192": "post_type_8071192",
    "1338406": "post_type_1338406",
    "5563439": "post_type_5563439",
    "TO DO": "post_type_to_do",
    "23939": "post_type_23939",
    "5563349": "post_type_5563349",
    "28931": "post_type_28931",
    "1480743": "post_type_1480743"
  }
}


