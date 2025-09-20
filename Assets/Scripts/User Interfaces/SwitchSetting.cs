using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Cysharp.Threading.Tasks;

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
            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleValue);
        }

        private void OnEnable()
        {
            OnEnableAsync().Forget();
        }

        private async UniTaskVoid OnEnableAsync()
        {
            await UniTask.WaitUntil(() => SaveDataManager.Instance != null && SaveDataManager.Instance.SettingsData != null);

            settingsData = SaveDataManager.Instance.SettingsData;

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
