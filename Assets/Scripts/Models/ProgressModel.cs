using System.Collections.Generic;
using Newtonsoft.Json;
using SaveSystem;

namespace Game
{
    public class ProgressModel : ISaveable
    {
        private class ProgressData
        {
            public int currentLevel;
            public HashSet<int> unlockedLevels;
            public Dictionary<int, int> starsPerLevel;
            public Dictionary<int, int> highScores;
        }

        private int _currentLevel;
        private HashSet<int> _unlockedLevels = new HashSet<int>();
        private Dictionary<int, int> _starsPerLevel = new Dictionary<int, int>();
        private Dictionary<int, int> _highScores = new Dictionary<int, int>();

        // ISaveable
        public string SaveKey => "progress";
        public int DataVersion => 1;

        public object ExportSaveData()
        {
            return new ProgressData
            {
                currentLevel = _currentLevel,
                unlockedLevels = _unlockedLevels,
                starsPerLevel = _starsPerLevel,
                highScores = _highScores
            };
        }

        public void ImportSaveData(string json, int version)
        {
            var data = JsonConvert.DeserializeObject<ProgressData>(json);
            if (data == null) return;

            _currentLevel = data.currentLevel;
            _unlockedLevels = data.unlockedLevels ?? new HashSet<int>();
            _starsPerLevel = data.starsPerLevel ?? new Dictionary<int, int>();
            _highScores = data.highScores ?? new Dictionary<int, int>();
        }

        public void ResetToDefault()
        {
            _currentLevel = 0;
            _unlockedLevels.Clear();
            _unlockedLevels.Add(0);
            _starsPerLevel.Clear();
            _highScores.Clear();
        }

        // Model API
        public int CurrentLevel => _currentLevel;

        public void SetCurrentLevel(int level)
        {
            _currentLevel = level;
            GameEvents.DataChanged.Publish();
        }

        public void UnlockLevel(int levelIndex)
        {
            if (_unlockedLevels.Add(levelIndex))
            {
                GameEvents.DataChanged.Publish();
            }
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            return _unlockedLevels.Contains(levelIndex);
        }

        public void SetStars(int levelIndex, int stars)
        {
            if (!_starsPerLevel.ContainsKey(levelIndex) || _starsPerLevel[levelIndex] < stars)
            {
                _starsPerLevel[levelIndex] = stars;
                GameEvents.DataChanged.Publish();
            }
        }

        public int GetStars(int levelIndex)
        {
            return _starsPerLevel.TryGetValue(levelIndex, out int stars) ? stars : 0;
        }

        public void SetHighScore(int levelIndex, int score)
        {
            if (!_highScores.ContainsKey(levelIndex) || _highScores[levelIndex] < score)
            {
                _highScores[levelIndex] = score;
                GameEvents.DataChanged.Publish();
            }
        }

        public int GetHighScore(int levelIndex)
        {
            return _highScores.TryGetValue(levelIndex, out int score) ? score : 0;
        }
    }
}
