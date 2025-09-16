using System;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SettingDataSO", menuName = "ScriptableObjects/SettingDataSO", order = 1)]
public class SettingDataSO : ScriptableObject
{
    [Serializable]
    public struct SliderSettingData
    {
        public string ID;
        public float min;
        public float max;
        public float defaultValue;
        public float step;
    }

    public SliderSettingData[] sliderSettingDataArray;
}
