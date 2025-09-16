using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Iztar.UserInterface
{
    public class SwitchSetting : SettingUIComponent
    {
        [Header("UI References")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI label; 

        [Header("Texts")]
        [SerializeField] private string trueText = "ON";
        [SerializeField] private string falseText = "OFF";

        private bool currentValue;

        private void Start()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleValue);
            }

            UpdateLabel();
        }

        private void ToggleValue()
        {
            currentValue = !currentValue;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label != null)
            {
                label.text = currentValue ? trueText : falseText;
            }
        }
    }
}
