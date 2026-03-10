# Task

Unity 2022.3.62f2 (LTS) — Developer case study showcasing a production-ready, modular Save/Load system with clean MVC architecture.

[Demo Video](https://drive.google.com/file/d/1wsqF8cOjUNy69mIIRdEKQlu3gAY5pj0I/view?usp=sharing)

## Architecture

Event-driven MVC pattern using `ModelLocator` for centralized data access and `GameEvents` for decoupled communication. Controllers extend `BaseController` (headless) or `BaseViewController` (with UI) from the local `Game.Framework` module. Infrastructure components like `SaveSlotFactory` handle object pooling and spawning, keeping Views free of instantiation logic.

**Namespaces:** `Game` (game-specific code) | `SaveSystem` (reusable save framework) | `SaveDemo` (demo UI) | `Game.Framework` (shared framework)

## Project Structure

```
Assets/Scripts/
├── Core/
│   ├── Framework/
│   │   ├── BaseController.cs      # Abstract controller with Init/Subscribe/Unsubscribe lifecycle
│   │   ├── BaseViewController.cs  # Controller with serialized BaseView reference
│   │   ├── BaseView.cs            # Abstract view with DOTween fade transitions
│   │   ├── GameEvent.cs           # Lightweight event system (Subscribe/Unsubscribe/Publish)
│   │   └── ModelLocator.cs        # Static type-based model registry
│   ├── BootLoader.cs              # Registers all models in ModelLocator
│   └── GameEvents.cs              # Centralized event definitions
├── Models/
│   ├── SaveModel.cs               # Runtime save metadata (not saveable)
│   ├── CurrencyModel.cs           # Coins & gems
│   ├── ProgressModel.cs           # Levels, stars, high scores
│   ├── AchievementModel.cs        # Achievement tracking
│   ├── SettingsModel.cs           # Audio, language preferences
│   └── StatisticsModel.cs         # Win/loss, streaks
├── Editor/
│   └── SaveDataEditorTools.cs     # Editor utilities for save data
└── Modules/
    ├── Save/
    │   ├── ISaveable.cs           # Interface for saveable models
    │   ├── SaveController.cs      # Orchestrates save/load operations
    │   ├── SaveConfig.cs          # ScriptableObject settings (slot count)
    │   ├── SaveData.cs            # Serialization data structures
    │   ├── SaveSerializer.cs      # Newtonsoft.Json + SHA256 checksum
    │   └── SaveFileHandler.cs     # Disk I/O and file management
    └── SaveDemo/
        ├── SaveDemoViewController.cs  # Demo controller with UI constraints
        ├── SaveDemoView.cs            # Demo UI view (receives slots from factory)
        ├── SaveSlotView.cs            # Individual slot button view
        └── SaveSlotFactory.cs         # Object-pooled slot spawning factory
```

## Demo Scene

Set up the SaveSystem scene manually in the Unity Editor with:
- A Canvas containing the SaveDemoPanel (with `SaveDemoViewController` + `SaveDemoView`)
- A BootLoader and SaveController GameObject
- A ScrollView with VerticalLayoutGroup for dynamic slot spawning
- A SaveSlotFactory component for object-pooled slot spawning

The demo lets you select save slots, modify game data (coins, gems, levels, stars), save/load, delete slots, and start new games — all wired to the real save system.

### Dynamic Slot System

Slot count is driven by `SaveConfig.MaxSlots` (configurable 1–10 in the ScriptableObject at `Assets/SOs/SaveConfig.asset`). Slots are managed by `SaveSlotFactory`, which uses an object pool (pre-warm, get, return) to efficiently recycle `SaveSlotView` instances from `ButtonSlotPrefab`. The factory owns the prefab and container references; `SaveDemoViewController` orchestrates slot creation while `SaveDemoView` only handles presentation.

#### Changing Slot Count

1. Open `Assets/SOs/SaveConfig.asset` in the Inspector
2. Adjust **Max Slots** (1–10)
3. Run the game — the system reconciles automatically on launch:
   - **Increasing**: New empty slots are added to the manifest
   - **Decreasing**: Excess slots and their save files are deleted
   - If the previously active slot no longer exists, it resets to no active slot

No data wipe or manual file cleanup is needed.

### UI Edge Cases Handled

| Edge Case | Behavior |
|-----------|----------|
| **Scene start, no slot selected** | All action/demo buttons disabled; only slot buttons are interactable |
| **Empty slot selected** | Only New Game button enabled |
| **Slot with save data selected** | New Game, Load, and Delete enabled; Save/demo buttons disabled until loaded |
| **Active slot selected** | All buttons enabled (Save, Load, Delete, New Game, demo data) |
| **Switching slots with unsaved changes** | Auto-saves the current active slot before switching |
| **Deleting the active slot** | Clears active slot index; data bar clears instantly; Save/demo buttons disable |
| **Data bar on non-active slot** | Shows slot summary ("Level: X \| Saved data - Load to view details") or "No data" for empty slots |
| **Scene restart with previous session** | Restores last active slot selection and displays its data automatically |
| **Manifest not yet created (first run)** | Creates default manifest with correct slot count and persists it |

## Editor Tools

Available under **Tools > Save Data**:

- **Open Save Folder** — reveals the persistent data directory in your file explorer
- **Delete All Save Data** — removes all save files and manifest (with confirmation)
- **Log Save File Paths** — prints save file names, sizes, and timestamps to the Console

## Save System

- **Multiple save slots** (configurable, 1–10) with manifest tracking
- **Auto-save** on application quit and pause (mobile backgrounding)
- **Auto-save on slot switch** when data is dirty
- **SHA256 checksum** validation on every load
- **Chunked format** — each model serializes independently via `ISaveable`
- **Newtonsoft.Json** for robust serialization (dictionaries, nulls, complex types)

## Save System Flow

### Startup

```
BootLoader.Awake()
    │
    ├── ModelLocator.Clear()
    └── RegisterModels()
            │
            ├── ModelLocator.Register(SaveModel)
            ├── ModelLocator.Register(CurrencyModel)
            ├── ModelLocator.Register(ProgressModel)
            └── ... (all models)

SaveController.Awake() [via BaseController]
    │
    ├── Init()
    │    ├── ModelLocator.Get<SaveModel>()
    │    ├── LoadManifest() ──→ save_manifest.json
    │    └── RegisterSaveable<T>() for each model
    │
    └── Subscribe()
         └── Subscribe to GameEvents
```

### Save

```
GameEvents.SaveRequested ──→ SaveController.Save(slot)
    │
    ├── BuildSaveData()
    │    └── foreach ISaveable ──→ ExportSaveData() ──→ SaveChunk
    │
    ├── SaveSerializer.Serialize()
    │    ├── Serialize chunks to JSON
    │    └── Compute SHA256 checksum ──→ header.checksum
    │
    ├── SaveFileHandler.WriteFile() ──→ save_slot_N.json
    │
    ├── UpdateSlotInfo() + SaveManifest() ──→ save_manifest.json
    │
    └── GameEvents.SaveCompleted
```

### Load

```
GameEvents.LoadRequested(slot) ──→ SaveController.Load(slot)
    │
    ├── SaveFileHandler.ReadFile() ←── save_slot_N.json
    │
    ├── SaveSerializer.Deserialize() ──→ SaveData
    │
    ├── ValidateChecksum()
    │    ├── ✓ Valid ──→ continue
    │    └── ✗ Invalid ──→ ResetAllToDefaults()
    │
    ├── ImportSaveData()
    │    └── foreach SaveChunk ──→ ISaveable.ImportSaveData(json, version)
    │
    └── GameEvents.LoadCompleted
```

## How to Extend

1. Create a model implementing `ISaveable` (`SaveKey`, `ExportSaveData()`, `ImportSaveData()`, `ResetToDefault()`)
2. Register it in `BootLoader.RegisterModels()` via `ModelLocator.Register(new YourModel())`

3. Add `RegisterSaveable<YourModel>()` in `SaveController.Init()`
