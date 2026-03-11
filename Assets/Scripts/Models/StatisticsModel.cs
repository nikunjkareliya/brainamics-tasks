using System;
using Newtonsoft.Json;
using SaveSystem;

namespace Game
{
    public class StatisticsModel : ISaveable
    {
        [Serializable]
        private class StatisticsData
        {
            public int totalGamesPlayed;
            public int totalWins;
            public int totalLosses;
            public int longestStreak;
            public int totalCoinsEarned;
        }

        private int _totalGamesPlayed;
        private int _totalWins;
        private int _totalLosses;
        private int _longestStreak;
        private int _totalCoinsEarned;
        private int _currentStreak;

        // ISaveable
        public string SaveKey => "statistics";
        public int DataVersion => 1;

        public object ExportSaveData()
        {
            return new StatisticsData
            {
                totalGamesPlayed = _totalGamesPlayed,
                totalWins = _totalWins,
                totalLosses = _totalLosses,
                longestStreak = _longestStreak,
                totalCoinsEarned = _totalCoinsEarned
            };
        }

        public void ImportSaveData(string json, int version)
        {
            var data = JsonConvert.DeserializeObject<StatisticsData>(json);
            if (data != null)
            {
                _totalGamesPlayed = data.totalGamesPlayed;
                _totalWins = data.totalWins;
                _totalLosses = data.totalLosses;
                _longestStreak = data.longestStreak;
                _totalCoinsEarned = data.totalCoinsEarned;
                _currentStreak = 0;
            }
        }

        public void ResetToDefault()
        {
            _totalGamesPlayed = 0;
            _totalWins = 0;
            _totalLosses = 0;
            _longestStreak = 0;
            _totalCoinsEarned = 0;
            _currentStreak = 0;
        }

        // Model API
        public int TotalGamesPlayed => _totalGamesPlayed;
        public int TotalWins => _totalWins;
        public int TotalLosses => _totalLosses;
        public int LongestStreak => _longestStreak;
        public int TotalCoinsEarned => _totalCoinsEarned;
        public int CurrentStreak => _currentStreak;

        public void RecordWin()
        {
            _totalGamesPlayed++;
            _totalWins++;
            _currentStreak++;
            if (_currentStreak > _longestStreak)
            {
                _longestStreak = _currentStreak;
            }
            GameEvents.DataChanged.Publish();
        }

        public void RecordLoss()
        {
            _totalGamesPlayed++;
            _totalLosses++;
            _currentStreak = 0;
            GameEvents.DataChanged.Publish();
        }

        public void RecordCoinsEarned(int amount)
        {
            _totalCoinsEarned += amount;
            GameEvents.DataChanged.Publish();
        }
    }
}
