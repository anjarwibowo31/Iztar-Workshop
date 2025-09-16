using Sirenix.OdinInspector;
using UnityEngine;

namespace Iztar.UserInterface
{
    public class SettingUI : MonoBehaviour
    {
        public static SettingUI Instance { get; private set; }

        [SerializeField] private SettingDataSO settingDataReference;

        [Title("All Setting Components")]
        [SerializeField] private SliderSetting[] sliderComponents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (settingDataReference == null)
            {
                Debug.LogError("SettingDataSO reference is not set in the inspector.");
                return;
            }

            BindSettingsToComponents();
        }

        private void BindSettingsToComponents()
        {
            foreach (var comp in sliderComponents)
            {
                if (comp == null) continue;

                var id = comp.GetSettingID();
                if (string.IsNullOrEmpty(id)) continue;

                var match = System.Array.Find(settingDataReference.sliderSettingDataArray, s => s.ID == id);
                if (!string.IsNullOrEmpty(match.ID))
                {
                    comp.AssignData(match);
                }
                else
                {
                    Debug.LogWarning($"No setting data found for ID: {id}", comp);
                }
            }
        }

        [Button]
        private void FetchAllComponentsInChildren()
        {
            sliderComponents = GetComponentsInChildren<SliderSetting>(true);
        }
    }
}
