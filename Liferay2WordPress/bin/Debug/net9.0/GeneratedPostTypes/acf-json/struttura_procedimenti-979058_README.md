# struttura procedimenti - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `979058`
**Structure ID:** `979059`
**ClassNameId:** `10109` - CMN - Contenuti
**Parent Menu:** CMN - Contenuti

## Description

<?xml version='1.0' encoding='UTF-8'?><root available-locales="it_IT" default-locale="it_IT"><Description language-id="it_IT">ATTENZIONE: Inserire la categoria a cui appartiene il procedimento</Description></root>

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

This post type has **105** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Data di creazione | `ddm-date` |  | - |
| Data di modifica | `ddm-date` |  | - |
| Riferimenti normativi utili | `ddm-text-html` |  | - |
| Area | `select` |  | - |
| - | `option` |  | - |
| Sindaco Metropolitano | `option` |  | - |
| Consiglio Metropolitano | `option` |  | - |
| Conferenza Metropolitana | `option` |  | - |
| Segretario Generale | `option` |  | - |
| Amministrativa Edilizia Istituzionale, Mobilità e Viabilità | `option` |  | - |
| Affari istituzionali, Gare, Stazione Unica Appaltante | `option` |  | - |
| Avvocatura | `option` |  | - |
| Pianificazione Territoriale, Urbanistica, Sviluppo – Valorizzazione e Tutela Ambientale | `option` |  | - |
| Risorse Umane, Innovazione e Qualità dei Servizi, Pari Opportunità | `option` |  | - |
| Servizi Finanziari | `option` |  | - |
| Tecnica Edilizia Istituzionale, Mobilità e Viabilità | `option` |  | - |
| Corpo di Polizia Metropolitana | `option` |  | - |
| Direzione Pianificazione Strategica e Politiche Comunitarie | `option` |  | - |
| Direzione | `select` |  | - |
| - | `option` |  | - |
| Segretario Generale | `option` |  | - |
| Pianificazione dei servizi e delle reti di trasporto | `option` |  | - |
| Amministrativa Patrimonio – Provveditorato | `option` |  | - |
| Amministrativa Scuole e Programmazione Scolastica | `option` |  | - |
| Amministrativa Strade e Viabilità | `option` |  | - |
| Supporto Organi istituzionali, Sindaco, Consiglio e Conferenza metropolitana, Affari Generali, Flussi Documentali, Anticorruzione, Trasparenza, Controlli | `option` |  | - |
| Gare e Contratti dell’Ente, Espropri, SUA | `option` |  | - |
| Legale 1 | `option` |  | - |
| Legale 2 | `option` |  | - |
| Pianificazione Territoriale – Urbanistica | `option` |  | - |
| Ambiente, Sviluppo del territorio, Sanzioni | `option` |  | - |
| politiche del personale, pari opportunità, qualità dei servizi | `option` |  | - |
| Trattamento Giuridico, Economico e Previdenziale | `option` |  | - |
| Sistemi Informativi integrati | `option` |  | - |
| programmazione finanziaria e bilancio | `option` |  | - |
| Contabilità ed Economato e Tributi | `option` |  | - |
| Partecipazioni e Controllo Analogo | `option` |  | - |
| Programmazione finanziaria e bilancio | `option` |  | - |
| progettazione – progetti speciali | `option` |  | - |
| Gestione Tecnica del Patrimonio | `option` |  | - |
| Gestione Tecnica Edilizia Scolastica | `option` |  | - |
| Gestione Tecnica Strade e Viabilità | `option` |  | - |
| CORPO POLIZIA METROPOLITANA | `option` |  | - |
| Pianificazione strategica e politiche comunitarie | `option` |  | - |
| Funzioni Statali e Regionali | `option` |  | - |
| Responsabile del procedimento (indicare anche recapiti telefonici e mail istituzionale) | `textarea` |  | - |
| organo competente | `select` |  | - |
| - | `option` |  | - |
| Consiglio | `option` |  | - |
| Dirigente | `option` |  | - |
| Sindaco | `option` |  | - |
| Tipologia di atto finale | `select` |  | - |
|  | `option` |  | - |
| Abilitazione | `option` |  | - |
| Approvazione | `option` |  | - |
| Assegnazione | `option` |  | - |
| Attestato | `option` |  | - |
| Attestazione | `option` |  | - |
| Atto | `option` |  | - |
| Autorizzazione/diniego | `option` |  | - |
| Certificazione | `option` |  | - |
| Concessione | `option` |  | - |
| Decreto | `option` |  | - |
| Deliberazione | `option` |  | - |
| Determinazione | `option` |  | - |
| Iscrizione | `option` |  | - |
| Licenza | `option` |  | - |
| Nulla Osta | `option` |  | - |
| Parere | `option` |  | - |
| Revoca | `option` |  | - |
| Tesserino | `option` |  | - |
| Modalità con le quali gli interessati possono ottenere le informazioni relative ai procedimenti in corso che li riguardino | `textarea` |  | - |
| termine per la conclusione del procedimento (giorni) | `ddm-integer` | ✓ | - |
| Indicare le ragioni per termini superiori ai 90 giorni | `textarea` |  | - |
| prevista dichiarazione sostitutiva? | `checkbox` |  | - |
| previsto il silenzio/assenso? | `checkbox` |  | - |
| Strumenti di tutela amministrativa e giurisdizionale | `ddm-text-html` |  | - |
| Modalità per l'effettuazione dei pagamenti | `textarea` |  | - |
| Identificativi del pagamento | `textarea` |  | - |
| soggetto a cui è attribuito il potere sostitutivo | `textarea` |  | - |
| avvio del procedimento | `select` |  | - |
| - | `option` |  | - |
| a richiesta | `option` |  | - |
| d'ufficio | `option` |  | - |
| Risultato delle indagini di customer satisfaction | `textarea` |  | - |
| Per informazioni | `textarea` |  | - |
| A rischio corruzione? | `checkbox` |  | - |
| Area di rischio | `select` |  | - |
| - | `option` |  | - |
| Acquisizione e progressione del personale | `option` |  | - |
| Contratti pubblici (già Macro area affidamento di lavori, servizi e forniture...) | `option` |  | - |
| Provvedimenti ampliativi della sfera giuridica dei destinatari privi di effetto economico diretto ed immediato per il destinatario | `option` |  | - |
| Provvedimenti ampliativi della sfera giuridica dei destinatari con effetto economico diretto ed immediato per il destinatario | `option` |  | - |
| Gestione delle entrate, delle spese e del patrimonio | `option` |  | - |
| Controlli, verifiche, ispezioni e sanzioni (già Macro area attività di controllo e irrogazione di sanzioni...) | `option` |  | - |
| Incarichi e nomine | `option` |  | - |
| Affari legali e contenzioso | `option` |  | - |
| Smaltimento dei rifiuti | `option` |  | - |
| Pianificazione urbanistica | `option` |  | - |
| Note | `ddm-text-html` |  | - |
| Allegato | `text` |  | - |
|  | `ddm-documentlibrary` |  | - |
| Collegamento alla Pagina | `ddm-link-to-page` |  | - |
| Collegamento Esterno | `text` |  | - |
| Collegamento alla pagina di accesso al servizio online | `ddm-link-to-page` |  | - |

