using System;
using Newtonsoft.Json;
using SaveSystem;
using UnityEngine;

namespace Game
{
    public class SettingsModel : ISaveable
    {
        [Serializable]
        private class SettingsData
        {
            public bool sfxEnabled;
            public bool musicEnabled;
            public float sfxVolume;
            public float musicVolume;
            public string language;
        }

        private bool _sfxEnabled = true;
        private bool _musicEnabled = true;
        private float _sfxVolume = 1f;
        private float _musicVolume = 1f;
        private string _language = "en";

        // ISaveable
        public string SaveKey => "settings";
        public int DataVersion => 1;

        public object ExportSaveData()
        {
            return new SettingsData
            {
                sfxEnabled = _sfxEnabled,
                musicEnabled = _musicEnabled,
                sfxVolume = _sfxVolume,
                musicVolume = _musicVolume,
                language = _language
            };
        }

        public void ImportSaveData(string json, int version)
        {
            var data = JsonConvert.DeserializeObject<SettingsData>(json);
            if (data != null)
            {
                _sfxEnabled = data.sfxEnabled;
                _musicEnabled = data.musicEnabled;
                _sfxVolume = data.sfxVolume;
                _musicVolume = data.musicVolume;
                _language = data.language;
            }
        }

        public void ResetToDefault()
        {
            _sfxEnabled = true;
            _musicEnabled = true;
            _sfxVolume = 1f;
            _musicVolume = 1f;
            _language = "en";
        }

        // Model API
        public bool SfxEnabled => _sfxEnabled;
        public bool MusicEnabled => _musicEnabled;
        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
        public string Language => _language;

        public void SetSfxEnabled(bool enabled)
        {
            _sfxEnabled = enabled;
            GameEvents.DataChanged.Publish();
        }

        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            GameEvents.DataChanged.Publish();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            GameEvents.DataChanged.Publish();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            GameEvents.DataChanged.Publish();
        }

        public void SetLanguage(string language)
        {
            _language = language;
            GameEvents.DataChanged.Publish();
        }
    }
}
