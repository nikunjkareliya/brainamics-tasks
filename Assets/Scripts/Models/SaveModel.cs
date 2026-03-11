using System.Collections.Generic;
using SaveSystem;

namespace Game
{
    /// <summary>
    /// Runtime metadata for the save system. Registered in ModelLocator.
    /// NOT saveable itself — it tracks which slot is active and slot info.
    /// </summary>
    public class SaveModel
    {
        private int _activeSlotIndex = -1;
        private List<SaveSlotInfo> _slots = new List<SaveSlotInfo>();
        private bool _isDirty;
        private float _lastSaveTime;

        public int ActiveSlotIndex => _activeSlotIndex;
        public List<SaveSlotInfo> Slots => _slots;
        public bool IsDirty => _isDirty;
        public float LastSaveTime => _lastSaveTime;

        public void SetActiveSlotIndex(int index)
        {
            _activeSlotIndex = index;
        }

        public void SetSlots(List<SaveSlotInfo> slots)
        {
            _slots = slots ?? new List<SaveSlotInfo>();
        }

        public void MarkDirty()
        {
            _isDirty = true;
        }

        public void ClearDirty()
        {
            _isDirty = false;
        }

        public void SetLastSaveTime(float time)
        {
            _lastSaveTime = time;
        }
    }
}
