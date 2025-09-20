using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Iztar.UserInterface
{
    public class SettingUI : MonoBehaviour
    {
        public static SettingUI Instance { get; private set; }

        [Title("All Setting Components")]
        [SerializeField] private SliderSetting[] sliderComponents;

        private SettingDataSO settingDataReference;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            OnEnableAsync().Forget();
        }

        public void ResetSaveData()
        {
            PopupChoiceWindow.Instance.Show("Confirm reset data?", onConfirm: () => SaveDataManager.Instance.ResetToDefault());
        }

        public void ExitToMainMenu()
        {
            PopupChoiceWindow.Instance.Show("Back to Main Menu?", onConfirm: () => SceneManager.LoadScene("MainMenu"));
        }

        public void AssignToShip()
        {
            if (GameManager.Instance.ActiveShip != null && settingDataReference != null)
            {
                GameManager.Instance.ActiveShip.SetUpFromSettingData(settingDataReference);
            }
        }

        private async UniTaskVoid OnEnableAsync()
        {
            await UniTask.WaitUntil(() => SaveDataManager.Instance.SettingsData != null);
            settingDataReference = SaveDataManager.Instance.SettingsData;

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
