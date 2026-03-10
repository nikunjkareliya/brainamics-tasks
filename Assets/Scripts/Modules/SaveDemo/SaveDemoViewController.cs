using System.Collections.Generic;
using Game;
using SaveSystem;
using Game.Framework;
using UnityEngine;

namespace SaveDemo
{
    /// <summary>
    /// Manages demo interactions between save system UI and models.
    /// </summary>
    public class SaveDemoViewController : BaseViewController
    {
        [SerializeField] private SaveSlotFactory _slotFactory;

        private SaveDemoView View => _viewObject as SaveDemoView;

        private SaveModel _saveModel;
        private CurrencyModel _currencyModel;
        private ProgressModel _progressModel;
        private int _selectedSlot = -1;

        protected override void Init()
        {
            _saveModel = ModelLocator.Get<SaveModel>();
            _currencyModel = ModelLocator.Get<CurrencyModel>();
            _progressModel = ModelLocator.Get<ProgressModel>();

            _slotFactory.PreWarm();
            InitSlots(_saveModel.Slots);
            _selectedSlot = _saveModel.ActiveSlotIndex;
            RefreshUI();
            RefreshButtonStates();
        }

        protected override void Subscribe()
        {
            GameEvents.SaveCompleted.Subscribe(HandleSaveCompleted);
            GameEvents.LoadCompleted.Subscribe(HandleLoadCompleted);
            GameEvents.SaveFailed.Subscribe(HandleSaveFailed);
            GameEvents.LoadFailed.Subscribe(HandleLoadFailed);

            View.OnSlotSelected += OnViewSlotSelected;
            View.OnSaveClicked += OnViewSaveClicked;
            View.OnLoadClicked += OnViewLoadClicked;
            View.OnDeleteClicked += OnViewDeleteClicked;
            View.OnNewGameClicked += OnViewNewGameClicked;
            View.OnAddCoinsClicked += OnViewAddCoinsClicked;
            View.OnAddGemsClicked += OnViewAddGemsClicked;
            View.OnAddLevelClicked += OnViewAddLevelClicked;
            View.OnAddStarsClicked += OnViewAddStarsClicked;
        }

        protected override void Unsubscribe()
        {
            GameEvents.SaveCompleted.Unsubscribe(HandleSaveCompleted);
            GameEvents.LoadCompleted.Unsubscribe(HandleLoadCompleted);
            GameEvents.SaveFailed.Unsubscribe(HandleSaveFailed);
            GameEvents.LoadFailed.Unsubscribe(HandleLoadFailed);

            if (View != null)
            {
                View.OnSlotSelected -= OnViewSlotSelected;
                View.OnSaveClicked -= OnViewSaveClicked;
                View.OnLoadClicked -= OnViewLoadClicked;
                View.OnDeleteClicked -= OnViewDeleteClicked;
                View.OnNewGameClicked -= OnViewNewGameClicked;
                View.OnAddCoinsClicked -= OnViewAddCoinsClicked;
                View.OnAddGemsClicked -= OnViewAddGemsClicked;
                View.OnAddLevelClicked -= OnViewAddLevelClicked;
                View.OnAddStarsClicked -= OnViewAddStarsClicked;
            }
        }

        // --- View Event Handlers ---

        private void OnViewSlotSelected(int slotIndex)
        {
            if (_saveModel.IsDirty && _saveModel.ActiveSlotIndex >= 0)
            {
                GameEvents.SaveRequested.Publish();
            }

            _selectedSlot = slotIndex;
            View.SetSelectedSlot(slotIndex);
            View.SetStatus($"Selected slot {slotIndex}");
            RefreshDataDisplay();
            RefreshButtonStates();
        }

        private void OnViewSaveClicked()
        {
            if (_saveModel.ActiveSlotIndex < 0)
            {
                View.SetStatus("No active slot. Select a slot and start a New Game first.");
                return;
            }

            GameEvents.SaveRequested.Publish();
        }

        private void OnViewLoadClicked()
        {
            int slot = GetSelectedOrActiveSlot();
            if (slot < 0) return;

            GameEvents.LoadRequested.Publish(slot);
        }

        private void OnViewDeleteClicked()
        {
            int slot = GetSelectedOrActiveSlot();
            if (slot < 0) return;

            GameEvents.DeleteSlotRequested.Publish(slot);

            if (slot == _saveModel.ActiveSlotIndex)
            {
                _saveModel.SetActiveSlotIndex(-1);
            }

            View.SetStatus($"Deleted slot {slot}");
            RefreshUI();
        }

        private void OnViewNewGameClicked()
        {
            int slot = GetSelectedOrActiveSlot();
            if (slot < 0) return;

            GameEvents.NewGameRequested.Publish(slot);
            View.SetStatus($"New game started in slot {slot}");
            RefreshUI();
        }

        private void OnViewAddCoinsClicked()
        {
            _currencyModel.AddCoins(100);
            View.SetStatus("+100 coins");
            RefreshDataDisplay();
        }

        private void OnViewAddGemsClicked()
        {
            _currencyModel.AddGems(10);
            View.SetStatus("+10 gems");
            RefreshDataDisplay();
        }

        private void OnViewAddLevelClicked()
        {
            int nextLevel = _progressModel.CurrentLevel + 1;
            _progressModel.SetCurrentLevel(nextLevel);
            _progressModel.UnlockLevel(nextLevel);
            View.SetStatus($"Level set to {nextLevel}");
            RefreshDataDisplay();
        }

        private void OnViewAddStarsClicked()
        {
            int level = _progressModel.CurrentLevel;
            int currentStars = _progressModel.GetStars(level);
            int newStars = (currentStars % 3) + 1;
            _progressModel.SetStars(level, newStars);
            View.SetStatus($"Set {newStars} stars on level {level}");
            RefreshDataDisplay();
        }

        // --- GameEvent Handlers ---

        private void HandleSaveCompleted()
        {
            View.SetStatus($"Saved to slot {_saveModel.ActiveSlotIndex} successfully");
            RefreshUI();
        }

        private void HandleLoadCompleted()
        {
            View.SetStatus($"Loaded slot {_saveModel.ActiveSlotIndex} successfully");
            RefreshUI();
        }

        private void HandleSaveFailed(string error)
        {
            View.SetStatus($"Save failed: {error}");
        }

        private void HandleLoadFailed(string error)
        {
            View.SetStatus($"Load failed: {error}");
        }

        // --- Helpers ---

        private int GetSelectedOrActiveSlot()
        {
            if (_selectedSlot >= 0)
                return _selectedSlot;

            int slot = _saveModel.ActiveSlotIndex;
            if (slot < 0)
            {
                View.SetStatus("No slot selected. Click a slot first.");
                return -1;
            }
            return slot;
        }

        private void RefreshUI()
        {
            View.UpdateSlots(_saveModel.Slots);

            if (_saveModel.ActiveSlotIndex >= 0)
            {
                View.SetSelectedSlot(_saveModel.ActiveSlotIndex);
            }

            RefreshDataDisplay();
            RefreshButtonStates();
        }

        private void RefreshButtonStates()
        {
            bool slotSelected = _selectedSlot >= 0;
            bool slotHasData = false;

            if (slotSelected)
            {
                var slots = _saveModel.Slots;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].slotIndex == _selectedSlot)
                    {
                        slotHasData = !slots[i].isEmpty;
                        break;
                    }
                }
            }

            bool hasActiveSlot = _saveModel.ActiveSlotIndex >= 0 && _selectedSlot == _saveModel.ActiveSlotIndex;
            View.SetButtonStates(slotSelected, slotHasData, hasActiveSlot);
        }

        private void RefreshDataDisplay()
        {
            if (_selectedSlot >= 0 && _selectedSlot == _saveModel.ActiveSlotIndex)
            {
                View.UpdateDataDisplay(
                    _currencyModel.Coins,
                    _currencyModel.Gems,
                    _progressModel.CurrentLevel,
                    _progressModel.GetStars(_progressModel.CurrentLevel)
                );
                return;
            }

            if (_selectedSlot >= 0)
            {
                var slots = _saveModel.Slots;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].slotIndex == _selectedSlot && !slots[i].isEmpty)
                    {
                        View.SetDataDisplayText($"Level: {slots[i].currentLevel}  |  Saved data - Load to view details");
                        return;
                    }
                }
            }

            View.SetDataDisplayText("No data");
        }

        private void InitSlots(List<SaveSlotInfo> slots)
        {
            _slotFactory.ReturnAll();

            var slotViews = new List<SaveSlotView>();
            for (int i = 0; i < slots.Count; i++)
            {
                SaveSlotView slotView = _slotFactory.Get();
                slotView.Init(slots[i]);
                slotViews.Add(slotView);
            }

            View.SetSlots(slotViews);
        }
    }
}
