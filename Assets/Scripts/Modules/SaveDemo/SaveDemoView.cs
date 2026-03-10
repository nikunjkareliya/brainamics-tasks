using System;
using System.Collections.Generic;
using SaveSystem;
using Game.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaveDemo
{
    /// <summary>
    /// Handles all UI presentation for the save system demo.
    /// Receives pre-created slot views from the controller via SetSlots.
    /// </summary>
    public class SaveDemoView : BaseView
    {
        [Header("Actions")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _newGameButton;

        [Header("Demo Data")]
        [SerializeField] private Button _addCoinsButton;
        [SerializeField] private Button _addGemsButton;
        [SerializeField] private Button _addLevelButton;
        [SerializeField] private Button _addStarsButton;

        [Header("Display")]
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _dataText;

        [Header("Slot Colors")]
        [SerializeField] private Color _selectedSlotColor = new Color(0.3f, 0.5f, 0.8f);
        [SerializeField] private Color _defaultSlotColor = new Color(0.25f, 0.25f, 0.35f);

        // Events for Controller
        public event Action<int> OnSlotSelected;
        public event Action OnSaveClicked;
        public event Action OnLoadClicked;
        public event Action OnDeleteClicked;
        public event Action OnNewGameClicked;
        public event Action OnAddCoinsClicked;
        public event Action OnAddGemsClicked;
        public event Action OnAddLevelClicked;
        public event Action OnAddStarsClicked;

        private List<SaveSlotView> _slotViews = new List<SaveSlotView>();
        private int _selectedSlot = -1;

        private void Awake()
        {
            _saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            _loadButton.onClick.AddListener(() => OnLoadClicked?.Invoke());
            _deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke());
            _newGameButton.onClick.AddListener(() => OnNewGameClicked?.Invoke());

            _addCoinsButton.onClick.AddListener(() => OnAddCoinsClicked?.Invoke());
            _addGemsButton.onClick.AddListener(() => OnAddGemsClicked?.Invoke());
            _addLevelButton.onClick.AddListener(() => OnAddLevelClicked?.Invoke());
            _addStarsButton.onClick.AddListener(() => OnAddStarsClicked?.Invoke());
        }

        /// <summary>
        /// Receives pre-created slot views from the Controller/Factory.
        /// Applies colors and subscribes to click events.
        /// </summary>
        public void SetSlots(List<SaveSlotView> slotViews)
        {
            for (int i = 0; i < _slotViews.Count; i++)
                _slotViews[i].OnSlotClicked -= HandleSlotClicked;

            _slotViews.Clear();
            _slotViews.AddRange(slotViews);

            for (int i = 0; i < _slotViews.Count; i++)
            {
                _slotViews[i].SetColors(_selectedSlotColor, _defaultSlotColor);
                _slotViews[i].OnSlotClicked += HandleSlotClicked;
            }
        }

        public void UpdateSlots(List<SaveSlotInfo> slots)
        {
            for (int i = 0; i < _slotViews.Count && i < slots.Count; i++)
            {
                _slotViews[i].UpdateDisplay(slots[i]);
            }

            HighlightSelectedSlot();
        }

        public void SetSelectedSlot(int index)
        {
            _selectedSlot = index;
            HighlightSelectedSlot();
        }

        public void SetStatus(string message)
        {
            _statusText.text = message;
        }

        public void UpdateDataDisplay(int coins, int gems, int level, int stars)
        {
            _dataText.text = $"Coins: {coins}  |  Gems: {gems}  |  Level: {level}  |  Stars: {stars}";
        }

        public void SetDataDisplayText(string text)
        {
            _dataText.text = text;
        }

        public void SetButtonStates(bool slotSelected, bool slotHasData, bool hasActiveSlot)
        {
            _newGameButton.interactable = slotSelected;
            _loadButton.interactable = slotSelected && slotHasData;
            _deleteButton.interactable = slotSelected && slotHasData;
            _saveButton.interactable = hasActiveSlot;

            _addCoinsButton.interactable = hasActiveSlot;
            _addGemsButton.interactable = hasActiveSlot;
            _addLevelButton.interactable = hasActiveSlot;
            _addStarsButton.interactable = hasActiveSlot;
        }

        private void HandleSlotClicked(int slotIndex)
        {
            _selectedSlot = slotIndex;
            OnSlotSelected?.Invoke(slotIndex);
            HighlightSelectedSlot();
        }

        private void HighlightSelectedSlot()
        {
            for (int i = 0; i < _slotViews.Count; i++)
            {
                _slotViews[i].SetSelected(_slotViews[i].SlotIndex == _selectedSlot);
            }
        }
    }
}
