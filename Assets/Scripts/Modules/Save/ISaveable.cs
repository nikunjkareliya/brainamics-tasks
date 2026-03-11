namespace SaveSystem
{
    /// <summary>
    /// Interface for Models that support save/load persistence.
    /// Models implement this to define what data gets serialized
    /// and how to restore state from saved data.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Unique key identifying this saveable in the save file.
        /// Must be stable across versions. Example: "currency", "progress"
        /// </summary>
        string SaveKey { get; }

        /// <summary>
        /// Current data version for this saveable's schema.
        /// Increment when the data shape changes.
        /// </summary>
        int DataVersion { get; }

        /// <summary>
        /// Export current model state to a JSON-serializable string.
        /// Called by SaveController when saving.
        /// </summary>
        object ExportSaveData();

        /// <summary>
        /// Import state from a previously saved JSON string.
        /// Called by SaveController when loading.
        /// </summary>
        /// <param name="json">The raw JSON string for this saveable's data chunk</param>
        /// <param name="version">The version of the data that was saved</param>
        void ImportSaveData(string json, int version);

        /// <summary>
        /// Reset this model to default/new-game state.
        /// Called when starting a new save slot or when data is corrupt.
        /// </summary>
        void ResetToDefault();
    }
}
