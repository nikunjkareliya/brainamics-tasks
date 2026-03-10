using System;
using System.Collections.Generic;

namespace SaveSystem
{
    /// <summary>
    /// Root container for an entire save file.
    /// This is the top-level object written to disk.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveHeader header;
        public List<SaveChunk> chunks;

        public SaveData()
        {
            header = new SaveHeader();
            chunks = new List<SaveChunk>();
        }
    }

    /// <summary>
    /// Metadata about the save file.
    /// </summary>
    [Serializable]
    public class SaveHeader
    {
        public int saveVersion;
        public string appVersion;
        public string saveDate;
        public string checksum;
    }

    /// <summary>
    /// A single model's persisted data, keyed by SaveKey.
    /// </summary>
    [Serializable]
    public class SaveChunk
    {
        public string key;
        public int version;
        public object data;
    }

    /// <summary>
    /// Metadata for a single save slot (shown in UI, stored in manifest).
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public string slotName;
        public string lastSaveDate;
        public int currentLevel;
        public float playTimeSeconds;
        public bool isEmpty;

        public SaveSlotInfo()
        {
            isEmpty = true;
            slotName = "";
            lastSaveDate = "";
        }

        public SaveSlotInfo(int index) : this()
        {
            slotIndex = index;
            slotName = $"Slot {index + 1}";
        }
    }

    /// <summary>
    /// Container for all slot metadata, stored in a separate manifest file.
    /// </summary>
    [Serializable]
    public class SaveManifest
    {
        public int lastActiveSlotIndex;
        public List<SaveSlotInfo> slots;

        public SaveManifest()
        {
            lastActiveSlotIndex = -1;
            slots = new List<SaveSlotInfo>();
        }
    }
}
