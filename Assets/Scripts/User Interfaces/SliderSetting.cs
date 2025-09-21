using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace Iztar.UserInterface
{
    public class SliderSetting : SettingUIComponent
    {
        [ShowInInspector, ReadOnly] protected SettingDataSO.SliderSettingData currentData;
        public SettingDataSO.SliderSettingData GetCurrentData() => currentData;

        [Header("UI References")]
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI label;

        private int decimals;
        private string format;

        public void AssignData(SettingDataSO.SliderSettingData data)
        {
            currentData = data;

            if (!string.IsNullOrEmpty(currentData.ID))
            {
                slider.minValue = currentData.min;
                slider.maxValue = currentData.max;
                slider.value = currentData.currentValue;

                float step = currentData.step > 0 ? currentData.step : 1f;
                decimals = GetDecimalPlaces(step);
                format = "F" + decimals;

                label.text = slider.value.ToString(format);
                slider.onValueChanged.AddListener(SnapToIncrement);
            }
        }

        private void SnapToIncrement(float value)
        {
            float min = slider.minValue;
            float max = slider.maxValue;
            float step = currentData.step > 0 ? currentData.step : 1f;

            float current = slider.value;
            float snapped;

            float stepsFromTop = Mathf.Round((max - value) / step);
            snapped = max - stepsFromTop * step;
            snapped = Mathf.Clamp(snapped, min, max);

            if (!Mathf.Approximately(snapped, current))
            {
                slider.SetValueWithoutNotify(snapped);
            }

            currentData.currentValue = snapped;

            label.text = snapped.ToString(format);
        }

        private int GetDecimalPlaces(float step)
        {
            int decimals = 0;
            while (step % 1f != 0f && decimals < 6)
            {
                step *= 10f;
                decimals++;
            }
            return decimals;
        }
    }
}
