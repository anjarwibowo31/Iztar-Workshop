using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Iztar.UserInterface
{
    public class SettingUIComponent : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] protected SettingDataSO.SliderSettingData currentData;

        [SerializeField] private string settingID;

        public void AssignData(SettingDataSO.SliderSettingData data)
        {
            currentData = data;
        }

        [Button]
        private void FetchGameObjectNameForID()
        {
            settingID = gameObject.name;
        }

        public string GetSettingID() => settingID;
        public SettingDataSO.SliderSettingData GetCurrentData() => currentData;
    }
}
