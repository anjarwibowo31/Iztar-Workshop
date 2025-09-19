using System;
using System.IO;
using UnityEngine;

public static class SaveDataUtility
{
    public static string SavePathInternal => SavePath; // biar bisa diakses GameManager
    private static string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    /// <summary>
    /// Simpan SettingDataSO ke JSON file (hanya currentValue + flag boolean).
    /// </summary>
    public static void SaveSettings(SettingDataSO setting)
    {
        if (setting == null)
        {
            Debug.LogError("SaveManager: SettingDataSO null!");
            return;
        }

        var save = new SettingDataSave
        {
            isUsingOneShotDash = setting.isUsingOneShotDash,
            sliderValues = new SliderValue[setting.sliderSettingDataArray.Length]
        };

        for (int i = 0; i < setting.sliderSettingDataArray.Length; i++)
        {
            save.sliderValues[i] = new SliderValue
            {
                ID = setting.sliderSettingDataArray[i].ID,
                currentValue = setting.sliderSettingDataArray[i].currentValue
            };
        }

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] Settings saved to {SavePath}");
    }

    /// <summary>
    /// Load JSON ke SettingDataSO (overwrite currentValue saja).
    /// </summary>
    public static void LoadSettings(SettingDataSO setting)
    {
        if (setting == null)
        {
            Debug.LogError("SaveManager: SettingDataSO null!");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"[SaveManager] No save file found at {SavePath}");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var save = JsonUtility.FromJson<SettingDataSave>(json);

        setting.isUsingOneShotDash = save.isUsingOneShotDash;

        foreach (var loaded in save.sliderValues)
        {
            for (int i = 0; i < setting.sliderSettingDataArray.Length; i++)
            {
                if (setting.sliderSettingDataArray[i].ID == loaded.ID)
                {
                    var s = setting.sliderSettingDataArray[i];
                    s.currentValue = loaded.currentValue;
                    setting.sliderSettingDataArray[i] = s;
                    break;
                }
            }
        }

        Debug.Log($"[SaveManager] Settings loaded from {SavePath}");
    }
}

[Serializable]
public class SettingDataSave
{
    public bool isUsingOneShotDash;
    public SliderValue[] sliderValues;
}

[Serializable]
public struct SliderValue
{
    public string ID;
    public float currentValue;
}
