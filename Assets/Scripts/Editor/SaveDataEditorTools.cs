using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu shortcuts for inspecting and managing save data files on disk.
/// Menu: Tools > Save Data
/// </summary>
public static class SaveDataEditorTools
{
    private const string SAVE_FILE_PATTERN = "save_slot_*";
    private const string MANIFEST_FILE = "save_manifest.json";

    [MenuItem("Tools/Save Data/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Save Data/Delete All Save Data")]
    public static void DeleteAllSaveData()
    {
        string path = Application.persistentDataPath;

        string[] saveFiles = Directory.GetFiles(path, "save_slot_*");
        string manifestPath = Path.Combine(path, MANIFEST_FILE);
        bool manifestExists = File.Exists(manifestPath);

        int totalFiles = saveFiles.Length + (manifestExists ? 1 : 0);

        if (totalFiles == 0)
        {
            EditorUtility.DisplayDialog("Delete Save Data", "No save files found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Delete Save Data",
            $"Delete {totalFiles} save file(s) from:\n{path}\n\nThis cannot be undone.",
            "Delete", "Cancel"))
        {
            return;
        }

        int deleted = 0;
        for (int i = 0; i < saveFiles.Length; i++)
        {
            File.Delete(saveFiles[i]);
            deleted++;
        }

        if (manifestExists)
        {
            File.Delete(manifestPath);
            deleted++;
        }

        Debug.Log($"[SaveDataTools] Deleted {deleted} save file(s) from {path}");
    }

    [MenuItem("Tools/Save Data/Log Save File Paths")]
    public static void LogSaveFilePaths()
    {
        string path = Application.persistentDataPath;
        Debug.Log($"[SaveDataTools] Save folder: {path}");

        if (!Directory.Exists(path))
        {
            Debug.Log("[SaveDataTools] Folder does not exist yet.");
            return;
        }

        string[] saveFiles = Directory.GetFiles(path, "save_*");

        if (saveFiles.Length == 0)
        {
            Debug.Log("[SaveDataTools] No save files found.");
            return;
        }

        for (int i = 0; i < saveFiles.Length; i++)
        {
            FileInfo info = new FileInfo(saveFiles[i]);
            Debug.Log($"  {info.Name} ({info.Length} bytes, modified {info.LastWriteTime})");
        }
    }
}
