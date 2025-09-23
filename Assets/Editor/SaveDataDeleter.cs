#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SaveDataDeleter : EditorWindow
{
    private string folderPath;
    private string fileName = "settings.json";

    [MenuItem("Iztar Tools/Save Data Deleter")]
    public static void ShowWindow()
    {
        GetWindow<SaveDataDeleter>("Save Data Deleter");
    }

    private void OnEnable()
    {
        folderPath = Application.persistentDataPath;
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON Tools", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Delete JSON File"))
        {
            DeleteJsonFile();
        }

        GUILayout.Space(20);
        GUILayout.Label("Save Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Save Data"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All PlayerPrefs deleted!");
        }
    }

    private void DeleteJsonFile()
    {
        string fullPath = Path.Combine(folderPath, fileName);

        if (File.Exists(fullPath))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                $"Delete this file?\n{fullPath}", "Yes", "Cancel"))
            {
                File.Delete(fullPath);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Deleted", "File deleted successfully!", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", $"File not found:\n{fullPath}", "OK");
        }
    }
}
#endif
