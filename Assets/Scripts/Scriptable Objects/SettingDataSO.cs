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

    public SliderSettingData[] sliderSettingDataArray;

    public bool isUsingOneShotDash = true;

    [SerializeField] private bool dashDefaultValue = true;

    public bool GetDashDefaultValue => dashDefaultValue;

    public void InitializeDefaults()
    {
        for (int i = 0; i < sliderSettingDataArray.Length; i++)
        {
            var s = sliderSettingDataArray[i];
            s.currentValue = s.GetDefaultValue;
            sliderSettingDataArray[i] = s;
        }
    }
}
