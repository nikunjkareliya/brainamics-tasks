using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Configuration for the save system.
    /// Used only by SaveController (single module), so direct [SerializeField] reference.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveConfig", menuName = "Config/Save Config")]
    public class SaveConfig : ScriptableObject
    {
        [Header("Slot Settings")]
        [Tooltip("Change takes effect on next launch. Existing saves preserved when increasing. Excess slot files auto-deleted when decreasing.")]
        [SerializeField, Range(1, 10)] private int _maxSlots = 3;

        public int MaxSlots => _maxSlots;
    }
}
