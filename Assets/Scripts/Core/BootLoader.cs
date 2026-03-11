using Game.Framework;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Registers all models in ModelLocator.
    /// Must execute before other controllers (DefaultExecutionOrder -100).
    /// Place on a persistent GameObject in the first scene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class BootLoader : MonoBehaviour
    {
        private void Awake()
        {
            ModelLocator.Clear();            
            RegisterModels();
        }

        private void RegisterModels()
        {
            ModelLocator.Register(new SaveModel());
            ModelLocator.Register(new CurrencyModel());
            ModelLocator.Register(new SettingsModel());
            ModelLocator.Register(new ProgressModel());
            ModelLocator.Register(new AchievementModel());
            ModelLocator.Register(new StatisticsModel());
        }
    }
}
