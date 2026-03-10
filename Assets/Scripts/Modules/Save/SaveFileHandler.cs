using System;
using System.IO;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Helper class for reading/writing save files to disk.    
    /// Writes to Application.persistentDataPath.
    /// </summary>
    public class SaveFileHandler
    {
        private readonly string _basePath;

        private const string SLOT_FILE_PREFIX = "save_slot_";
        private const string SLOT_FILE_EXTENSION = ".json";
        private const string MANIFEST_FILE_NAME = "save_manifest.json";

        public SaveFileHandler()
        {
            _basePath = Application.persistentDataPath;
        }

        public string GetSlotFilePath(int slotIndex)
        {
            return Path.Combine(_basePath, $"{SLOT_FILE_PREFIX}{slotIndex}{SLOT_FILE_EXTENSION}");
        }

        public string GetManifestFilePath()
        {
            return Path.Combine(_basePath, MANIFEST_FILE_NAME);
        }

        public bool WriteFile(string filePath, string content)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveFileHandler] Failed to write file '{filePath}': {e.Message}");
                return false;
            }
        }

        public string ReadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveFileHandler] Failed to read file '{filePath}': {e.Message}");
                return null;
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveFileHandler] Failed to delete file '{filePath}': {e.Message}");
                return false;
            }
        }

    }
}
