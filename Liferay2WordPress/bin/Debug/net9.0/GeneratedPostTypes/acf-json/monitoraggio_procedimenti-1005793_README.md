# monitoraggio_procedimenti - CMN Custom Post Type

**Generation Mode:** `ACF_JSON`
**Generated from Liferay Structure:** `1005793`
**Structure ID:** `1005794`
**ClassNameId:** `10109` - CMN - Contenuti
**Parent Menu:** CMN - Contenuti

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

This post type has **53** custom fields:

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| Direzione (inserire nel titolo il contenuto di questo campo) | `select` |  | - |
| Segretario Generale | `option` |  | - |
| (Direzione Gestione Contenzioso Amministrativo) Direzione Legale 3 | `option` |  | - |
| Amministrativa Patri(Direzione Gestione Contenzioso Civile) Direzione Legale 2 | `option` |  | - |
| (Direzione Gestione Contenzioso Lavoro / Penale) Direzione Legale 4 | `option` |  | - |
| Controllo Strategico - Capo di Gabinetto | `option` |  | - |
| Corpo di Polizia Provinciale | `option` |  | - |
| Direzione Affari Generali - Flussi Documentali | `option` |  | - |
| Direzione Amministrativa - Gestione Funzionamento Edifici Scolastici di 2° Grado | `option` |  | - |
| Direzione Amministrativa Ambiente | `option` |  | - |
| Direzione Amministrativa del Patrimonio / Prevenzione e Protezione / Rete Telefonica Fissa e Mobile | `option` |  | - |
| Direzione Amministrativa Viabilità | `option` |  | - |
| Direzione Attività Produttive (Turismo, Commercio, Artigianato, Agricoltura) | `option` |  | - |
| Direzione Autorizzazione e Controllo Del Trasporto | `option` |  | - |
| Direzione Cilo Integrato dei Rifiuti Tutela del Suolo Bonifica Siti, Risorse Idriche | `option` |  | - |
| Direzione del Consiglio | `option` |  | - |
| Direzione della Giunta | `option` |  | - |
| Direzione Diritto allo Studio / Educazione Permanente | `option` |  | - |
| Direzione Finanze, Investimenti, Tributi Investimenti e Patto di Stabilità | `option` |  | - |
| Direzione Gestione Bilancio, Rendicontazione Bilancio Consolidato Contabilità | `option` |  | - |
| Direzione Innovazione Organizzativa (CUG, Pari Opportunità Supporto ai Comuni Strandard di Qualità) | `option` |  | - |
| Direzione Interventi Edilizia Scolastica I | `option` |  | - |
| Direzione Interventi Edilizia Scolastica II | `option` |  | - |
| Direzione Interventi Viabilità | `option` |  | - |
| Direzione Mobilità Metropolitana | `option` |  | - |
| Direzione Partecipate Controllo Analogo | `option` |  | - |
| Direzione Patrocinio Innanzi alle Giurisdizioni Superiori - Consulenze e Pareri dell'Ente. | `option` |  | - |
| Direzione Pianificazione delle Reti di Trasporto | `option` |  | - |
| Direzione Pianificazione Territoriale e delle Reti Infrastrutturali | `option` |  | - |
| Direzione Politiche del Lavoro - Servizi per l'Impiego, Immigrazione, Osservatorio Mercato del Lavoro | `option` |  | - |
| Direzione Politiche del Personale - Implementazione dell'Organizzazione in Materia di Enti Locali. | `option` |  | - |
| Direzione Politiche per la Sicurezza | `option` |  | - |
| Direzione Progettazione Edilizia Scolastica | `option` |  | - |
| Direzione Progettazione Viabilità | `option` |  | - |
| Direzione Programmazione della Rete Scolastica Relativa alle Scuole | `option` |  | - |
| Direzione Programmazione e Controllo Equilibri Finanziari | `option` |  | - |
| Direzione Provveditorato, Economato | `option` |  | - |
| Direzione Sanzioni | `option` |  | - |
| Direzione Sistema Informativo ed Innovazione Tecnologica / Reti Telefoniche | `option` |  | - |
| Direzione Solidarietà Sociale, Interventi per le Famiglie, Politiche Giovanili | `option` |  | - |
| Direzione Stazione Unica Appalti, Servizi, Contratti | `option` |  | - |
| Direzione Strutturazione e Pianificazione dei Servizi Pubblici di Interesse Generale di Ambito Metropolitano | `option` |  | - |
| Direzione Tecnica del Patrimonio | `option` |  | - |
| Direzione Trattamento Giuridico, Economico e Previdenziale del Personale | `option` |  | - |
| Direzione Tutela delle Coste - Risorsa Mare | `option` |  | - |
| Descrizione procedimento (Cliccare iconcina + per aggiungere un nuovo procedimento) | `ddm-text-html` | ✓ | - |
| Responsabile | `textarea` |  | - |
| Numero totale delle pratiche | `ddm-integer` |  | - |
| Tempo massimo stabilito di durata del procedimento | `ddm-integer` |  | - |
| Numero totale pratiche che hanno superato il limite prefissato | `ddm-integer` |  | - |
| Motivo sforamento tempi | `textarea` |  | - |
| % delle pratiche che hanno superato il limite prefissato | `ddm-integer` |  | - |
| Tempo medio di durata del procedimento | `ddm-integer` |  | - |

