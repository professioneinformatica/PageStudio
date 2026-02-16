# PageStudio

## Panoramica del Progetto

PageStudio è un'applicazione di desktop publishing cross-platform, simile ad Affinity Publisher, costruita con .NET 10.0. Supporta Windows, macOS e Linux su architetture x86 e ARM.

## Stack Tecnologico

| Tecnologia | Ruolo |
|---|---|
| .NET 10.0 | Framework principale |
| .NET MAUI | Applicazione desktop cross-platform |
| Blazor Server / WebAssembly | Applicazione web |
| SkiaSharp 3.119.1 | Motore grafico 2D |
| PdfSharpCore 1.3.67 | Generazione PDF |
| SixLabors.ImageSharp 3.1.11 | Elaborazione immagini |
| Jint 4.1.0 | Motore JavaScript per formule |
| Esprima 3.0.5 | Parser JavaScript per validazione formule |
| xUnit 2.9.3 | Framework di testing |

## Architettura

Il progetto segue un'architettura a livelli con separazione netta tra logica di business e interfaccia utente.

```
┌─────────────────────────────────┐
│  UI Layer (MAUI / Blazor)       │
│  Views, Components, ViewModels  │
└──────────────┬──────────────────┘
               │
┌──────────────▼──────────────────┐
│  Business Logic (Core)          │
│  Document Model, Parametric     │
│  Engine, Event System           │
└──────────────┬──────────────────┘
               │
┌──────────────▼──────────────────┐
│  Graphics / Export Layer        │
│  SkiaSharp, PDF, Image Export   │
└─────────────────────────────────┘
```

### Pattern di Design Utilizzati

- **MVVM** - Separazione UI e testabilità
- **Dependency Injection** - Gestione servizi tramite Microsoft.Extensions.DependencyInjection e Scrutor
- **Command Pattern** - Operazioni annullabili (predisposto)
- **Observer Pattern** - Notifiche di cambiamento documento tramite eventi
- **Factory Pattern** - Creazione elementi e contesti grafici
- **Lazy Evaluation** - Proprietà dinamiche calcolate solo all'accesso
- **Composite Pattern** - Gerarchia di elementi con relazioni parent-child

## Struttura del Repository

```
PageStudio/
├── PageStudio.Core/                  # Logica di business (platform-agnostic)
│   ├── Graphics/                     # Astrazione rendering grafico
│   ├── Features/
│   │   ├── EventsManagement/         # Sistema di eventi (dispatcher e publisher)
│   │   └── ParametricProperties/     # Motore parametrico, proprietà dinamiche, formule
│   ├── Models/
│   │   ├── Abstractions/             # IPageElement, PageElement
│   │   ├── ContainerPageElements/    # TextElement, ImageElement, GroupElement, Layer
│   │   ├── Documents/                # IDocument, Document
│   │   └── Page/                     # IPage, Page
│   ├── Interfaces/                   # IGraphicsContext, IExportService, ecc.
│   ├── Services/                     # ExportService, DocumentsRepository
│   └── Export/                       # Opzioni di esportazione (PDF, PNG, JPG)
│
├── PageStudio.Desktop/               # Applicazione desktop (MAUI)
├── PageStudio.Web/                   # Applicazione web (Blazor Server)
├── PageStudio.Web.Client/            # Client WebAssembly (Blazor)
├── PageStudio.Tests/                 # Unit test (xUnit)
├── PageStudio.sln                    # File soluzione Visual Studio
└── README.md                         # Documentazione architetturale
```

## Modello del Documento

### Gerarchia Principale

- **IDocument** - Contenitore di pagine con metadati e impostazioni DPI
- **IPage** - Pagina individuale con dimensioni, margini e livelli
- **IPageElement** - Interfaccia base per tutti gli elementi posizionabili
- **ILayer** - Gestione livelli per ordinamento z-index

### Elementi Disponibili

| Elemento | Descrizione |
|---|---|
| **TextElement** | Testo con famiglia font, dimensione, stile, colore e allineamento |
| **ImageElement** | Supporto immagini raster con codifica base64 |
| **GroupElement** | Contenitore per raggruppare altri elementi |
| **Layer** | Gestione z-index per l'ordine di rendering |

## Sistema di Proprietà Parametriche

Una delle funzionalità più avanzate: consente proprietà dinamiche degli elementi tramite formule JavaScript.

### Componenti Chiave

- **ParametricEngine** - Motore centrale che gestisce le proprietà parametriche
- **DynamicProperty\<T\>** - Proprietà parametrica generica con valutazione lazy
- **JsFormula** - Rappresentazione di formule JavaScript
- **DependencyGraph** - Traccia le dipendenze tra proprietà
- **EvaluationContext** - Contesto di valutazione JavaScript
- **SymbolTable** - Registro dei simboli per riferimenti tra elementi
- **FormulaTranslator** - Traduce JavaScript in formule eseguibili

### Esempi di Formule

```javascript
// Riferimento a proprietà di altri elementi
ItemA.Width * 0.5

// Funzioni matematiche JavaScript
Math.sqrt(ItemA.X * ItemA.X + ItemA.Y * ItemA.Y)

// Espressioni condizionali
ItemA.Opacity > 0.5 ? true : false
```

### Caratteristiche

- Riferimenti tra elementi per nome (es. `Header.Width + 20`)
- Supporto completo alle funzioni `Math` di JavaScript
- Tracciamento automatico delle dipendenze con valutazione lazy
- Rilevamento e prevenzione delle dipendenze circolari
- Binding cross-element delle proprietà

## Sistema Grafico e di Rendering

- **IGraphicsContext** - Livello di astrazione per operazioni di rendering
- **GraphicsContext** - Implementazione basata su SkiaSharp
- Supporto per: rettangoli, cerchi, percorsi, immagini, testo, trasformazioni
- Hit-testing e handle di selezione (sistema a 8 handle)
- Trasformazioni di coordinate (traslazione, rotazione, scala)

## Servizi di Esportazione

| Formato | Tipo |
|---|---|
| PDF | Vettoriale (SkiaSharp + PdfSharpCore) |
| PNG | Raster |
| JPG | Raster |
| BMP | Raster |
| WebP | Raster |

Opzioni configurabili: risoluzione, qualità, rapporto d'aspetto, dimensioni. Supporto per esportazione pagina singola o documento completo.

## Sistema di Eventi

- **IEvent** - Interfaccia base per gli eventi
- **EventPublisher** - Meccanismo di pubblicazione eventi
- **EventDispatcher** - Routing e gestione eventi
- Notifiche di cambio z-index e altre modifiche al documento

## Interazioni Canvas

- **CanvasDocumentInteractor** - Interazioni specifiche del canvas
- Selezione e manipolazione elementi
- Pan e zoom
- Hit testing per la selezione
- Gestione pagine

## Formati Pagina e Layout

- **PageFormat** - Dimensioni pagina predefinite (A4, Letter, ecc.)
- **Margins** - Margini per pagine ed elementi
- **Units** - Supporto per centimetri e pollici
- Configurazione DPI (72, 96, 300+ DPI)

## Stato dell'Implementazione

### Completato

- Architettura completa delle interfacce
- Modello documento/pagina/elemento
- Motore proprietà parametriche
- Astrazione contesto grafico con SkiaSharp
- Servizio di esportazione (PDF, PNG, JPG)
- Sistema di gestione eventi
- Elementi testo e immagine
- Gestione livelli
- Interazioni canvas e zoom

### In Sviluppo / Pianificato

- Interfaccia utente desktop (integrazione MAUI)
- Componenti applicazione web (Blazor)
- Formattazione avanzata del testo
- Strumenti di disegno vettoriale
- Sistema di template e pagine master
- Collaborazione in tempo reale (SignalR)

## Build e Configurazione

- **File soluzione:** `PageStudio.sln`
- **Target framework:** net10.0 (tutti i progetti)
- **Configurazioni:** Debug/Release su Any CPU, x64, x86
- **Nullable reference types:** abilitati
- **Implicit usings:** abilitati

## Convenzioni di Sviluppo

- **Separation of Concerns** - Codice platform-specific isolato con compilazione condizionale
- **Interface-First Design** - Tutti i servizi principali definiti come interfacce
- **Guard Clauses** - Validazione input tramite Ardalis.GuardClauses
- **Async-First** - Operazioni asincrone in tutto lo stack
- **Testing** - Unit test con xUnit
