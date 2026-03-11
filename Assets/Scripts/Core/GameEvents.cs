using Game.Framework;

namespace Game
{
    /// <summary>
    /// Centralized gameplay event definitions.
    /// SharedEvents = UI/app flow. GameEvents = gameplay mechanics.
    /// </summary>
    public static class GameEvents
    {
        // Save System
        public static readonly GameEvent SaveRequested = new GameEvent();
        public static readonly GameEvent<int> LoadRequested = new GameEvent<int>();
        public static readonly GameEvent<int> DeleteSlotRequested = new GameEvent<int>();
        public static readonly GameEvent<int> NewGameRequested = new GameEvent<int>();
        public static readonly GameEvent SaveCompleted = new GameEvent();
        public static readonly GameEvent LoadCompleted = new GameEvent();
        public static readonly GameEvent<string> SaveFailed = new GameEvent<string>();
        public static readonly GameEvent<string> LoadFailed = new GameEvent<string>();

        /// <summary>
        /// Fired by saveable models when their data changes.
        /// SaveController listens to mark the save state as dirty.
        /// </summary>
        public static readonly GameEvent DataChanged = new GameEvent();
    }
}
