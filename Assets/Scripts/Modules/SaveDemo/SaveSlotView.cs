using System;
using SaveSystem;
using Game.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaveDemo
{
    /// <summary>
    /// Manages individual save slot button visuals and click interaction.
    /// Attached to each spawned slot prefab instance.
    /// </summary>
    public class SaveSlotView : BaseView, IPoolable
    {
        [SerializeField] private TMP_Text _slotText;
        [SerializeField] private Button _button;

        private int _slotIndex;
        private Color _selectedColor = new Color(0.3f, 0.5f, 0.8f);
        private Color _defaultColor = new Color(0.25f, 0.25f, 0.35f);

        public int SlotIndex => _slotIndex;

        public event Action<int> OnSlotClicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnSlotClicked?.Invoke(_slotIndex));
        }

        public void Init(SaveSlotInfo slotInfo)
        {
            _slotIndex = slotInfo.slotIndex;
            UpdateDisplay(slotInfo);
        }

        public void UpdateDisplay(SaveSlotInfo slotInfo)
        {
            if (slotInfo.isEmpty)
            {
                _slotText.text = $"Slot {slotInfo.slotIndex}\n<size=80%><color=#888>Empty</color></size>";
            }
            else
            {
                _slotText.text = $"Slot {slotInfo.slotIndex}\n<size=80%>Level: {slotInfo.currentLevel}\n{FormatDate(slotInfo.lastSaveDate)}</size>";
            }
        }

        public void SetSelected(bool selected)
        {
            _button.image.color = selected ? _selectedColor : _defaultColor;
        }

        public void OnPoolGet() { }

        public void OnPoolReturn()
        {
            OnSlotClicked = null;
            _slotIndex = -1;
            SetSelected(false);
        }

        public void SetColors(Color selectedColor, Color defaultColor)
        {
            _selectedColor = selectedColor;
            _defaultColor = defaultColor;
        }

        private string FormatDate(string isoDate)
        {
            if (DateTime.TryParse(isoDate, out DateTime date))
            {
                return date.ToLocalTime().ToString("MMM dd, HH:mm");
            }
            return isoDate;
        }
    }
}
