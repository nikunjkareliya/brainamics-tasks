using System;
using System.Collections.Generic;
using Game;
using Newtonsoft.Json;
using Game.Framework;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Headless controller managing all save/load operations.
    /// Coordinates between ISaveable models (via ModelLocator) and disk I/O.
    /// </summary>
    public class SaveController : BaseController
    {
        [SerializeField] private SaveConfig _config;

        private SaveModel _saveModel;
        private SaveFileHandler _fileHandler;
        private SaveSerializer _serializer;
        private List<ISaveable> _saveables;

        protected override void Init()
        {
            _saveModel = ModelLocator.Get<SaveModel>();
            _fileHandler = new SaveFileHandler();
            _serializer = new SaveSerializer();
            _saveables = new List<ISaveable>();

            LoadManifest();

            // Register all saveable models
            RegisterSaveable<CurrencyModel>();
            RegisterSaveable<SettingsModel>();
            RegisterSaveable<ProgressModel>();
            RegisterSaveable<AchievementModel>();
            RegisterSaveable<StatisticsModel>();
        }

        protected override void Subscribe()
        {
            GameEvents.SaveRequested.Subscribe(HandleSaveRequested);
            GameEvents.LoadRequested.Subscribe(HandleLoadRequested);
            GameEvents.DeleteSlotRequested.Subscribe(HandleDeleteSlotRequested);
            GameEvents.NewGameRequested.Subscribe(HandleNewGameRequested);
            GameEvents.DataChanged.Subscribe(HandleDataChanged);
        }

        protected override void Unsubscribe()
        {
            GameEvents.SaveRequested.Unsubscribe(HandleSaveRequested);
            GameEvents.LoadRequested.Unsubscribe(HandleLoadRequested);
            GameEvents.DeleteSlotRequested.Unsubscribe(HandleDeleteSlotRequested);
            GameEvents.NewGameRequested.Unsubscribe(HandleNewGameRequested);
            GameEvents.DataChanged.Unsubscribe(HandleDataChanged);
        }

        // --- Public API ---

        /// <summary>
        /// Save current model state to the specified slot.
        /// </summary>
        public bool Save(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.MaxSlots)
            {
                Debug.LogError($"[SaveController] Invalid slot index: {slotIndex}");
                GameEvents.SaveFailed.Publish($"Invalid slot index: {slotIndex}");
                return false;
            }

            try
            {
                // Build SaveData from all ISaveable models
                SaveData saveData = BuildSaveData();

                // Serialize to JSON (checksum is computed inside)
                string json = _serializer.Serialize(saveData);

                // Write to disk
                string filePath = _fileHandler.GetSlotFilePath(slotIndex);
                if (!_fileHandler.WriteFile(filePath, json))
                {
                    GameEvents.SaveFailed.Publish($"Failed to write save file for slot {slotIndex}");
                    return false;
                }

                // Update manifest
                UpdateSlotInfo(slotIndex, saveData);
                SaveManifest();

                // Update runtime state
                _saveModel.SetActiveSlotIndex(slotIndex);
                _saveModel.ClearDirty();
                _saveModel.SetLastSaveTime(Time.unscaledTime);

                Debug.Log($"[SaveController] Saved to slot {slotIndex} successfully.");
                GameEvents.SaveCompleted.Publish();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveController] Save failed for slot {slotIndex}: {e.Message}");
                GameEvents.SaveFailed.Publish(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Load data from the specified slot and populate all ISaveable models.
        /// </summary>
        public bool Load(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.MaxSlots)
            {
                Debug.LogError($"[SaveController] Invalid slot index: {slotIndex}");
                GameEvents.LoadFailed.Publish($"Invalid slot index: {slotIndex}");
                return false;
            }

            try
            {
                // Read file from disk
                string filePath = _fileHandler.GetSlotFilePath(slotIndex);
                string json = _fileHandler.ReadFile(filePath);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning($"[SaveController] No save data found for slot {slotIndex}.");
                    GameEvents.LoadFailed.Publish($"No save data for slot {slotIndex}");
                    return false;
                }

                // Deserialize
                SaveData saveData = _serializer.Deserialize(json);
                if (saveData == null || !_serializer.ValidateChecksum(saveData))
                {
                    ResetAllToDefaults(slotIndex);
                    return false;
                }

                // Import data into models
                ImportSaveData(saveData);

                // Update runtime state
                _saveModel.SetActiveSlotIndex(slotIndex);
                _saveModel.ClearDirty();

                Debug.Log($"[SaveController] Loaded slot {slotIndex} successfully.");
                GameEvents.LoadCompleted.Publish();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveController] Load failed for slot {slotIndex}: {e.Message}");
                GameEvents.LoadFailed.Publish(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete a save slot (file + manifest entry).
        /// </summary>
        public void DeleteSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.MaxSlots)
            {
                Debug.LogError($"[SaveController] Invalid slot index: {slotIndex}");
                return;
            }

            _fileHandler.DeleteFile(_fileHandler.GetSlotFilePath(slotIndex));

            // Reset slot info in manifest
            List<SaveSlotInfo> slots = _saveModel.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].slotIndex == slotIndex)
                {
                    slots[i] = new SaveSlotInfo(slotIndex);
                    break;
                }
            }

            SaveManifest();
            Debug.Log($"[SaveController] Deleted slot {slotIndex}.");
        }

        /// <summary>
        /// Reset all ISaveable models to defaults for a new game.
        /// </summary>
        public void NewGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.MaxSlots)
            {
                Debug.LogError($"[SaveController] Invalid slot index: {slotIndex}");
                return;
            }

            for (int i = 0; i < _saveables.Count; i++)
            {
                _saveables[i].ResetToDefault();
            }

            _saveModel.SetActiveSlotIndex(slotIndex);
            _saveModel.ClearDirty();

            // Save immediately to create the slot file
            Save(slotIndex);

            Debug.Log($"[SaveController] New game started in slot {slotIndex}.");
        }

        /// <summary>
        /// Check if a save slot has data on disk.
        /// </summary>
        public bool HasSaveData(int slotIndex)
        {
            return _fileHandler.FileExists(_fileHandler.GetSlotFilePath(slotIndex));
        }

        // --- Application Lifecycle ---

        private void OnApplicationQuit()
        {
            SaveIfDirty();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveIfDirty();
            }
        }

        private void SaveIfDirty()
        {
            if (_saveModel.IsDirty && _saveModel.ActiveSlotIndex >= 0)
            {
                Save(_saveModel.ActiveSlotIndex);
            }
        }

        // --- Event Handlers ---

        private void HandleSaveRequested()
        {
            if (_saveModel.ActiveSlotIndex >= 0)
            {
                Save(_saveModel.ActiveSlotIndex);
            }
            else
            {
                Debug.LogWarning("[SaveController] Save requested but no active slot.");
            }
        }

        private void HandleLoadRequested(int slotIndex)
        {
            Load(slotIndex);
        }

        private void HandleDeleteSlotRequested(int slotIndex)
        {
            DeleteSlot(slotIndex);
        }

        private void HandleNewGameRequested(int slotIndex)
        {
            NewGame(slotIndex);
        }

        private void HandleDataChanged()
        {
            _saveModel.MarkDirty();
        }

        // --- Internal Methods ---

        private void RegisterSaveable<T>() where T : class, ISaveable, new()
        {
            T model = ModelLocator.Get<T>();
            if (model != null)
            {
                _saveables.Add(model);
            }
        }

        private SaveData BuildSaveData()
        {
            SaveData saveData = new SaveData();
            saveData.header.saveVersion = 1;
            saveData.header.appVersion = Application.version;
            saveData.header.saveDate = DateTime.UtcNow.ToString("o");

            for (int i = 0; i < _saveables.Count; i++)
            {
                ISaveable saveable = _saveables[i];
                SaveChunk chunk = new SaveChunk
                {
                    key = saveable.SaveKey,
                    version = saveable.DataVersion,
                    data = saveable.ExportSaveData()
                };
                saveData.chunks.Add(chunk);
            }

            return saveData;
        }

        private void ImportSaveData(SaveData saveData)
        {
            for (int i = 0; i < saveData.chunks.Count; i++)
            {
                SaveChunk chunk = saveData.chunks[i];
                ISaveable saveable = FindSaveableByKey(chunk.key);

                if (saveable != null)
                {
                    try
                    {
                        string chunkJson = JsonConvert.SerializeObject(chunk.data);
                        saveable.ImportSaveData(chunkJson, chunk.version);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveController] Failed to import data for '{chunk.key}': {e.Message}. Resetting to default.");
                        saveable.ResetToDefault();
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveController] No saveable found for key '{chunk.key}'. Skipping.");
                }
            }
        }

        private ISaveable FindSaveableByKey(string key)
        {
            for (int i = 0; i < _saveables.Count; i++)
            {
                if (_saveables[i].SaveKey == key)
                    return _saveables[i];
            }
            return null;
        }

        private void ResetAllToDefaults(int slotIndex)
        {
            Debug.LogWarning($"[SaveController] Resetting all models to defaults for slot {slotIndex}.");
            for (int i = 0; i < _saveables.Count; i++)
            {
                _saveables[i].ResetToDefault();
            }

            _saveModel.SetActiveSlotIndex(slotIndex);
            _saveModel.ClearDirty();
            GameEvents.LoadFailed.Publish($"Save data corrupt for slot {slotIndex}. Reset to defaults.");
        }

        // --- Manifest Management ---

        private void LoadManifest()
        {
            string manifestPath = _fileHandler.GetManifestFilePath();
            string json = _fileHandler.ReadFile(manifestPath);

            SaveManifest manifest;
            if (!string.IsNullOrEmpty(json))
            {
                manifest = JsonConvert.DeserializeObject<SaveManifest>(json);
            }
            else
            {
                manifest = CreateDefaultManifest();
            }

            if (manifest == null)
            {
                manifest = CreateDefaultManifest();
            }

            _saveModel.SetSlots(manifest.slots);
            _saveModel.SetActiveSlotIndex(manifest.lastActiveSlotIndex);

            // Persist manifest after model is populated so SaveManifest() reads correct data
            if (string.IsNullOrEmpty(json))
            {
                SaveManifest();
            }

            ReconcileManifestSlots();
        }

        private void SaveManifest()
        {
            SaveManifest manifest = new SaveManifest
            {
                lastActiveSlotIndex = _saveModel.ActiveSlotIndex,
                slots = _saveModel.Slots
            };

            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            _fileHandler.WriteFile(_fileHandler.GetManifestFilePath(), json);
        }

        private void ReconcileManifestSlots()
        {
            List<SaveSlotInfo> slots = _saveModel.Slots;
            int maxSlots = _config.MaxSlots;

            // Remove excess slots (and delete their files)
            for (int i = slots.Count - 1; i >= maxSlots; i--)
            {
                _fileHandler.DeleteFile(_fileHandler.GetSlotFilePath(slots[i].slotIndex));
                slots.RemoveAt(i);
            }

            // Add missing slots
            for (int i = slots.Count; i < maxSlots; i++)
            {
                slots.Add(new SaveSlotInfo(i));
            }

            // Clamp activeSlotIndex if it's now out of range
            if (_saveModel.ActiveSlotIndex >= maxSlots)
            {
                _saveModel.SetActiveSlotIndex(-1);
            }

            SaveManifest();
        }

        private SaveManifest CreateDefaultManifest()
        {
            SaveManifest manifest = new SaveManifest();
            for (int i = 0; i < _config.MaxSlots; i++)
            {
                manifest.slots.Add(new SaveSlotInfo(i));
            }
            return manifest;
        }

        private void UpdateSlotInfo(int slotIndex, SaveData saveData)
        {
            List<SaveSlotInfo> slots = _saveModel.Slots;

            // Find or create slot info
            SaveSlotInfo slotInfo = null;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].slotIndex == slotIndex)
                {
                    slotInfo = slots[i];
                    break;
                }
            }

            if (slotInfo == null)
            {
                slotInfo = new SaveSlotInfo(slotIndex);
                slots.Add(slotInfo);
            }

            slotInfo.isEmpty = false;
            slotInfo.lastSaveDate = saveData.header.saveDate;

            // Try to extract display info from progress chunk
            for (int i = 0; i < saveData.chunks.Count; i++)
            {
                if (saveData.chunks[i].key == "progress")
                {
                    string chunkJson = JsonConvert.SerializeObject(saveData.chunks[i].data);
                    ProgressSlotData progressData = JsonConvert.DeserializeObject<ProgressSlotData>(chunkJson);
                    if (progressData != null)
                    {
                        slotInfo.currentLevel = progressData.currentLevel;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Minimal data class to extract currentLevel from progress chunk for manifest display.
        /// </summary>
        [Serializable]
        private class ProgressSlotData
        {
            public int currentLevel;
        }
    }
}
