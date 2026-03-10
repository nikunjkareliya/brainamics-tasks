using System.Collections.Generic;
using UnityEngine;

namespace SaveDemo
{
    /// <summary>
    /// Responsible for spawning and recycling SaveSlotView prefab instances
    /// using an object pool. Owns the prefab and container references.
    /// </summary>
    public class SaveSlotFactory : MonoBehaviour
    {
        [SerializeField] private SaveSlotView _slotPrefab;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private int _preWarmCount = 3;

        private readonly Queue<SaveSlotView> _pool = new Queue<SaveSlotView>();
        private readonly List<SaveSlotView> _activeSlots = new List<SaveSlotView>();

        /// <summary>
        /// Pre-instantiates slots into the pool for immediate use.
        /// </summary>
        public void PreWarm()
        {
            for (int i = 0; i < _preWarmCount; i++)
            {
                SaveSlotView slot = CreateSlotInstance();
                slot.gameObject.SetActive(false);
                _pool.Enqueue(slot);
            }
        }

        /// <summary>
        /// Gets a slot from the pool or creates a new one if the pool is empty.
        /// </summary>
        public SaveSlotView Get()
        {
            SaveSlotView slot;

            if (_pool.Count > 0)
            {
                slot = _pool.Dequeue();
            }
            else
            {
                slot = CreateSlotInstance();
            }

            slot.gameObject.SetActive(true);
            _activeSlots.Add(slot);
            return slot;
        }

        /// <summary>
        /// Returns a slot to the pool for reuse.
        /// </summary>
        public void Return(SaveSlotView slot)
        {
            if (slot == null) return;

            slot.ResetSlot();
            slot.gameObject.SetActive(false);
            _activeSlots.Remove(slot);
            _pool.Enqueue(slot);
        }

        /// <summary>
        /// Returns all active slots to the pool.
        /// </summary>
        public void ReturnAll()
        {
            for (int i = _activeSlots.Count - 1; i >= 0; i--)
            {
                SaveSlotView slot = _activeSlots[i];
                slot.ResetSlot();
                slot.gameObject.SetActive(false);
                _pool.Enqueue(slot);
            }
            _activeSlots.Clear();
        }

        private SaveSlotView CreateSlotInstance()
        {
            return Instantiate(_slotPrefab, _slotContainer);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _activeSlots.Count; i++)
            {
                if (_activeSlots[i] != null)
                    Destroy(_activeSlots[i].gameObject);
            }
            _activeSlots.Clear();

            while (_pool.Count > 0)
            {
                SaveSlotView slot = _pool.Dequeue();
                if (slot != null)
                    Destroy(slot.gameObject);
            }
        }
    }
}
