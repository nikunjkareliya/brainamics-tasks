using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SaveSystem;
using UnityEngine;

namespace Game
{
    public class AchievementModel : ISaveable
    {
        [Serializable]
        private class AchievementData
        {
            public List<AchievementEntry> achievements;
        }

        [Serializable]
        private class AchievementEntry
        {
            public string id;
            public bool isUnlocked;
            public string unlockDate;
            public float progress;
        }

        private Dictionary<string, AchievementState> _achievements = new Dictionary<string, AchievementState>();

        private class AchievementState
        {
            public bool isUnlocked;
            public string unlockDate;
            public float progress;
        }

        // ISaveable
        public string SaveKey => "achievements";
        public int DataVersion => 1;

        public object ExportSaveData()
        {
            var data = new AchievementData { achievements = new List<AchievementEntry>() };
            foreach (var kvp in _achievements)
            {
                data.achievements.Add(new AchievementEntry
                {
                    id = kvp.Key,
                    isUnlocked = kvp.Value.isUnlocked,
                    unlockDate = kvp.Value.unlockDate,
                    progress = kvp.Value.progress
                });
            }
            return data;
        }

        public void ImportSaveData(string json, int version)
        {
            var data = JsonConvert.DeserializeObject<AchievementData>(json);
            if (data?.achievements == null) return;

            _achievements.Clear();
            for (int i = 0; i < data.achievements.Count; i++)
            {
                var entry = data.achievements[i];
                _achievements[entry.id] = new AchievementState
                {
                    isUnlocked = entry.isUnlocked,
                    unlockDate = entry.unlockDate,
                    progress = entry.progress
                };
            }
        }

        public void ResetToDefault()
        {
            _achievements.Clear();
        }

        // Model API
        public bool IsUnlocked(string achievementId)
        {
            return _achievements.TryGetValue(achievementId, out var state) && state.isUnlocked;
        }

        public float GetProgress(string achievementId)
        {
            return _achievements.TryGetValue(achievementId, out var state) ? state.progress : 0f;
        }

        public void SetProgress(string achievementId, float progress)
        {
            if (!_achievements.ContainsKey(achievementId))
            {
                _achievements[achievementId] = new AchievementState();
            }

            _achievements[achievementId].progress = Mathf.Clamp01(progress);
            GameEvents.DataChanged.Publish();
        }

        public void Unlock(string achievementId)
        {
            if (!_achievements.ContainsKey(achievementId))
            {
                _achievements[achievementId] = new AchievementState();
            }

            if (!_achievements[achievementId].isUnlocked)
            {
                _achievements[achievementId].isUnlocked = true;
                _achievements[achievementId].progress = 1f;
                _achievements[achievementId].unlockDate = DateTime.UtcNow.ToString("o");
                GameEvents.DataChanged.Publish();
            }
        }
    }
}
