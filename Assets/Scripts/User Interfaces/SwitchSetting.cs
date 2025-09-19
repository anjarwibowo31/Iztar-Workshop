using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Iztar.UserInterface
{
    public class SwitchSetting : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI label;

        [Header("Texts")]
        [SerializeField] private string trueText = "ON";
        [SerializeField] private string falseText = "OFF";

        private SettingDataSO settingsData;

        private void Awake()
        {
            settingsData = SaveDataManager.Instance.SettingsData;

            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleValue);
        }

        private void OnEnable()
        {
            UpdateLabel();
        }

        private void ToggleValue()
        {
            settingsData.isUsingOneShotDash = !settingsData.isUsingOneShotDash;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label != null)
                label.text = settingsData.isUsingOneShotDash ? trueText : falseText;
        }
    }
}
