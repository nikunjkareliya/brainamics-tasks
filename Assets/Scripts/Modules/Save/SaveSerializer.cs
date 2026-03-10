using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Helper class for JSON serialization/deserialization of SaveData and checksum validation.    
    /// Uses Newtonsoft.Json for robust serialization (supports dictionaries, nulls, etc.).
    /// </summary>
    public class SaveSerializer
    {
        public string Serialize(SaveData saveData)
        {
            string chunksJson = SerializeChunks(saveData);
            saveData.header.checksum = ComputeChecksum(chunksJson);
            return JsonConvert.SerializeObject(saveData, Formatting.Indented);
        }

        public SaveData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[SaveSerializer] Attempted to deserialize null or empty JSON.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSerializer] Failed to deserialize save data: {e.Message}");
                return null;
            }
        }

        public bool ValidateChecksum(SaveData saveData)
        {
            if (saveData == null || saveData.header == null)
                return false;

            string chunksJson = SerializeChunks(saveData);
            string computedChecksum = ComputeChecksum(chunksJson);
            return computedChecksum == saveData.header.checksum;
        }

        private string ComputeChecksum(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Serialize only the chunks portion for checksum computation.
        /// This ensures the checksum covers the actual save data, not the header.
        /// </summary>
        private string SerializeChunks(SaveData saveData)
        {
            StringBuilder sb = new StringBuilder();
            if (saveData.chunks != null)
            {
                for (int i = 0; i < saveData.chunks.Count; i++)
                {
                    var chunk = saveData.chunks[i];
                    sb.Append(chunk.key);
                    sb.Append(chunk.version);
                    sb.Append(JsonConvert.SerializeObject(chunk.data));
                }
            }
            return sb.ToString();
        }
    }
}
