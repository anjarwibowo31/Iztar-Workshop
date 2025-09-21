using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingDataSO", menuName = "ScriptableObjects/SettingDataSO", order = 1)]
public class SettingDataSO : ScriptableObject
{
    [Serializable]
    public class SliderSettingData
    {
        public string ID;
        public float min;
        public float max;
        public float step;
        public float currentValue;

        [SerializeField] private float defaultValue;

        public float GetDefaultValue => defaultValue;
    }

    [Serializable]
    public class SwitchSettingData
    {
        public string ID;
        public bool currentValue;

        [SerializeField] private bool defaultValue;

        public bool GetDefaultValue => defaultValue;
    }

    public SliderSettingData[] sliderSettingDataArray;
    public SwitchSettingData[] switchSettingDataArray;

    public void InitializeDefaults()
    {
        for (int i = 0; i < sliderSettingDataArray.Length; i++)
        {
            var sliderSetting = sliderSettingDataArray[i];
            sliderSetting.currentValue = sliderSetting.GetDefaultValue;
            sliderSettingDataArray[i] = sliderSetting;
        }

        for (int i = 0; i < switchSettingDataArray.Length; i++)
        {
            var switchSetting = switchSettingDataArray[i];
            switchSetting.currentValue = switchSetting.GetDefaultValue;
            switchSettingDataArray[i] = switchSetting;
        }
    }
}
