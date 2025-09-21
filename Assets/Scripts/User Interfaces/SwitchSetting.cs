using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Iztar.UserInterface
{
    public class SwitchSetting : SettingUIComponent
    {
        [ShowInInspector, ReadOnly] protected SettingDataSO.SwitchSettingData currentData;
        public SettingDataSO.SwitchSettingData GetCurrentData() => currentData;

        [Header("UI References")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI label;

        [Header("Texts")]
        [SerializeField] private string trueText = "ON";
        [SerializeField] private string falseText = "OFF";

        private Func<bool> getter;
        private Action<bool> setter;

        private void Awake()
        {
            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleValue);
        }

        public void Initialize(Func<bool> getter, Action<bool> setter)
        {
            this.getter = getter;
            this.setter = setter;

            UpdateLabel();
        }

        private void ToggleValue()
        {
            if (getter == null || setter == null) return;

            bool current = getter();
            setter(!current);

            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label != null && getter != null)
                label.text = getter() ? trueText : falseText;
        }

        internal void AssignData(SettingDataSO.SwitchSettingData match)
        {
            if (match == null) return;

            currentData = match;

            // inject getter & setter ke Initialize
            Initialize(
                getter: () => currentData.currentValue,
                setter: val => currentData.currentValue = val
            );
        }

    }
}
