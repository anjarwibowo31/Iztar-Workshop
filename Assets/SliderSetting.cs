using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Iztar.UserInterface
{
    public class SliderSetting : SettingUIComponent
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI label;

        private int decimals;
        private string format;

        private void Start()
        {
            if (!string.IsNullOrEmpty(currentData.ID))
            {
                slider.minValue = currentData.min;
                slider.maxValue = currentData.max;
                slider.value = currentData.defaultValue;

                // 🔑 Tentukan jumlah desimal berdasarkan step
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

            // Snap berbasis max
            float stepsFromTop = Mathf.Round((max - value) / step);
            snapped = max - stepsFromTop * step;
            snapped = Mathf.Clamp(snapped, min, max);

            if (!Mathf.Approximately(snapped, current))
            {
                slider.SetValueWithoutNotify(snapped);
            }

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
