using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    public event Action OnDataChanged;


    [SerializeField] private SettingDataSO settingsData;

    public SettingDataSO SettingsData => settingsData;
    public Dictionary<string, SettingDataSO.SliderSettingData> SliderSettingsDataDict { get; private set; }
    public Dictionary<string, SettingDataSO.SwitchSettingData> SwitchSettingsDataDict { get; private set; }


    public string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadOrInitialize();
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        SliderSettingsDataDict = new Dictionary<string, SettingDataSO.SliderSettingData>();
        foreach (var setting in settingsData.sliderSettingDataArray)
        {
            SliderSettingsDataDict[setting.ID] = setting;
        }

        SwitchSettingsDataDict = new Dictionary<string, SettingDataSO.SwitchSettingData>();
        foreach (var setting in settingsData.switchSettingDataArray)
        {
            SwitchSettingsDataDict[setting.ID] = setting;
        }
    }

    /// <summary>
    /// Load settings kalau ada file, kalau tidak initialize ke default.
    /// </summary>
    public void LoadOrInitialize()
    {
        OnDataChanged?.Invoke();

        if (File.Exists(SavePath))
        {
            SaveDataUtility.LoadSettings(settingsData);
            InitializeDictionary();
        }
        else
        {
            Debug.Log("[SaveDataManager] No save found, initializing with default values");
            settingsData.InitializeDefaults();

            SaveDataUtility.SaveSettings(settingsData);
            InitializeDictionary();
        }
    }

    /// <summary>
    /// Simpan setting saat ini ke file JSON.
    /// </summary>
    public void Save()
    {
        OnDataChanged?.Invoke();

        if (settingsData == null)
        {
            Debug.LogError("[SaveDataManager] settingsData is null, cannot save!");
            return;
        }
        SaveDataUtility.SaveSettings(settingsData);
        InitializeDictionary();
    }

    /// <summary>
    /// Reset ke default value lalu simpan.
    /// </summary>
    [Button]
    public void ResetToDefault()
    {
        InitializeDictionary();

        if (settingsData == null)
        {
            Debug.LogError("[SaveDataManager] settingsData is null, cannot reset!");
            return;
        }

        settingsData.InitializeDefaults();

        SaveDataUtility.SaveSettings(settingsData);
        Debug.Log("[SaveDataManager] Settings reset to default and saved.");
    }
}
