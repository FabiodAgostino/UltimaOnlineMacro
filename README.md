# UltimaOnlineMacro

> Sistema di automazione avanzato per Ultima Online con Machine Learning e controllo remoto

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Framework-blue.svg)]()
[![ML](https://img.shields.io/badge/Machine%20Learning-YOLO-orange.svg)]()
[![MQTT](https://img.shields.io/badge/MQTT-IoT-green.svg)]()

## 📖 Descrizione

UltimaOnlineMacro è un sistema di automazione completo per Ultima Online che utilizza tecnologie avanzate come Machine Learning, Computer Vision e IoT per automatizzare gameplay, monitoraggio e controllo remoto.

## ✨ Funzionalità Principali

### 🎮 Automazione Macro
- **Hotkey personalizzabili** con modificatori (Ctrl, Alt, Shift)
- **Timer configurabile** per intervalli di esecuzione
- **Auto-restart** del client in caso di macrocheck
- **Gestione automatica** picconi e attrezzi

### 🤖 Machine Learning & Computer Vision
- **YOLO Object Detection** per riconoscimento muli e oggetti
- **Template Matching** con OpenCV per picconi
- **OCR Tesseract** per lettura status bar (stamina, peso)
- **Preprocessing immagini** per migliore accuratezza

### 📊 Monitoraggio Intelligente
- **Parsing journal logs** in tempo reale
- **Rilevamento eventi** (macrocheck, troppo peso, stamina bassa)
- **Status monitoring** automatico tramite OCR
- **Sistema notifiche** con livelli di severità

### 📱 Controllo Remoto
- **Server MQTT** integrato per comunicazione IoT
- **App mobile** companion per controllo remoto
- **Notifiche push** su dispositivi mobili
- **QR Code** per configurazione rapida

### 🎯 Gestione Regioni UI
- **Overlay trasparente** per selezione aree schermo
- **Configurazione visuale** di zaino, paperdoll, status
- **Salvataggio automatico** impostazioni
- **Validazione regioni** selezionate

## 🛠️ Requisiti di Sistema

| Componente | Versione Minima |
|------------|------------------|
| Windows | 10 (1903) |
| .NET | 8.0 |
| RAM | 4 GB |
| Storage | 500 MB |
| GPU | DirectX 11 compatibile |

### Dipendenze Esterne
- **Tesseract OCR** - Per riconoscimento testo
- **OpenCV (EmguCV)** - Computer vision
- **YOLO Model** - Object detection
- **ClassicUO Client** - Client Ultima Online

## 🚀 Configurazione Iniziale

### 1. Setup Percorsi
1. Avviare **UltimaOnlineMacro**
2. Andare in **Impostazioni** → **Seleziona File Macro**
3. Navigare fino a `The Miracle\Data\Profiles\[account]\[personaggio]\macros.xml`
4. L'app configurerà automaticamente il journal path

### 2. Configurazione Regioni
1. Selezionare **Regioni** → **Seleziona Zaino**
2. Disegnare un rettangolo attorno alla finestra zaino
3. Ripetere per **Paperdoll** e **Status Bar**
4. Testare il riconoscimento con **Refresh Screen**

### 3. Setup Macro
1. Scegliere **hotkey** e **modificatori**
2. Impostare **delay** tra esecuzioni (2000-10000ms)
3. Configurare **animali da soma** (Mulo/Lama)
4. Abilitare **AutoFuria** se disponibile

## 📱 Utilizzo

### Avvio Automazione
1. Configurare tutte le regioni necessarie
2. Impostare hotkey e delay
3. Posizionarsi nel gioco nella posizione desiderata
4. Premere START per avviare l'automazione

### Controllo Remoto Mobile
1. Generare QR Code dall'app desktop
2. Scansionare con UOMacroMobile
3. Monitorare notifiche in tempo reale
4. Controllare START/STOP da remoto

### Gestione Eventi Automatici
- **Macrocheck** → Auto-restart client e riconnessione
- **Troppo peso** → Notifica e pausa temporanea
- **Stamina bassa** → Pausa automatica 60 secondi
- **Piccone rotto** → Equipaggiamento automatico nuovo piccone

## 🏗️ Architettura del Sistema

UltimaOnlineMacro/
├── MainWindow/                 # Interfaccia WPF principale
├── AutoClicker/               # Core automazione
│   ├── Models/                # Modelli dati (Pg, Macro, Regions)
│   ├── Services/              # Servizi business logic
│   │   ├── SendInputService   # Simulazione input
│   │   ├── ProcessService     # Gestione processi
│   │   ├── TesseractService   # OCR text recognition
│   │   └── MacroManipulator   # Gestione file macro XML
│   └── Utils/                 # Utilità e helper
├── LogManager/                # Sistema logging
├── OverlayWindow/             # Selezione regioni UI
└── UltimaOnlineObjectDetector/ # Machine Learning
├── YoloDetector.cs        # Object detection C#
├── Trainer/               # Addestramento modelli Python
└── Analyzer/              # Analisi performance modelli


### Stack Tecnologico

#### Core Application
- **WPF .NET 8** - Interfaccia utente desktop
- **MVVM Pattern** - Architettura presentation layer
- **Dependency Injection** - Gestione servizi

#### Computer Vision & ML
- **EmguCV 4.10** - Computer vision operations
- **Tesseract 4.1** - OCR text recognition
- **YOLO v8** - Object detection neural network
- **ImageSharp** - Image processing pipeline

#### Automazione & Input
- **Win32 API** - Low-level input simulation
- **Windows Hooks** - Keyboard/mouse monitoring
- **Process Manipulation** - Client management

#### Comunicazione
- **MQTTnet** - IoT messaging protocol
- **Serilog** - Structured logging
- **System.Text.Json** - Data serialization

### Model Performance
- **Precision**: 90%+ su dataset test
- **Recall**: 85%+ per oggetti target
- **Inference Time**: <100ms su GPU
- **Model Size**: ~6MB (ottimizzato ONNX)

## 📋 Note Tecniche

> **Importante**: 
> - Richiede **amministratore** per hooks low-level
> - **Antivirus** potrebbero rilevare false positive per automation
> - **Tesseract** deve essere installato separatamente
> - **GPU CUDA** opzionale ma consigliata per ML

### Compatibilità Client
- ✅ **ClassicUO** (raccomandato)
- ✅ **Enhanced Client** (limitato)
- ❌ **Classic Client** (non supportato)

## 🚨 Avvertenze Legali

⚠️ **Questo software è solo per scopi educativi e di ricerca**

- Verificare **ToS del server** prima dell'uso
- **Non utilizzare** su server che vietano automazione
- **Rischio ban** permanente dell'account
- **Nessuna responsabilità** degli sviluppatori per conseguenze

## 🤝 Contributi

I contributi sono benvenuti! Aree di interesse:
- 🔬 **Miglioramenti ML** - Nuovi modelli e dataset
- 🎯 **Computer Vision** - Algoritmi detection più accurati
- 📱 **Mobile App** - Funzionalità aggiuntive
- 🐛 **Bug Fix** - Stabilità e performance


---
