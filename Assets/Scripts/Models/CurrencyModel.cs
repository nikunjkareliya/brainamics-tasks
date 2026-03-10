using System;
using Newtonsoft.Json;
using SaveSystem;

namespace Game
{
    public class CurrencyModel : ISaveable
    {
        [Serializable]
        private class CurrencyData
        {
            public int coins;
            public int gems;
        }

        private int _coins;
        private int _gems;

        // ISaveable
        public string SaveKey => "currency";
        public int DataVersion => 1;

        public object ExportSaveData()
        {
            return new CurrencyData { coins = _coins, gems = _gems };
        }

        public void ImportSaveData(string json, int version)
        {
            var data = JsonConvert.DeserializeObject<CurrencyData>(json);
            if (data != null)
            {
                _coins = data.coins;
                _gems = data.gems;
            }
        }

        public void ResetToDefault()
        {
            _coins = 0;
            _gems = 0;
        }

        // Model API
        public int Coins => _coins;
        public int Gems => _gems;

        public void AddCoins(int amount)
        {
            _coins += amount;
            GameEvents.DataChanged.Publish();
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            GameEvents.DataChanged.Publish();
        }

        public bool TrySpendCoins(int amount)
        {
            if (_coins < amount) return false;
            _coins -= amount;
            GameEvents.DataChanged.Publish();
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (_gems < amount) return false;
            _gems -= amount;
            GameEvents.DataChanged.Publish();
            return true;
        }
    }
}
