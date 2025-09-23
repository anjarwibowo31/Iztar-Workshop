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
        [SerializeField] private SwitchSetting[] switchComponents; // 🔥 tambahkan switch

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

        public void OpenSettingUI()
        {
            GameManager.Instance.PauseGame();
        }

        public void CloseSettingUI()
        {
            GameManager.Instance.ResumeGame();
        }

        public void ResetSaveData()
        {
            PopupChoiceWindow.Instance.Show("Confirm reset data?", onConfirm: () =>
            {
                SaveDataManager.Instance.ResetToDefault();
                BindSettingsToComponents();
            });
        }

        public void ExitToMainMenu()
        {
            PopupChoiceWindow.Instance.Show("Back to Main Menu?", onConfirm: () => SceneManager.LoadScene("MainMenu"));
        }

        public void AssignToShip()
        {
            if (GameManager.Instance.ActiveShip != null && settingDataReference != null)
            {
                GameManager.Instance.ActiveShip.SetUpFromSettingData();
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
            // === SLIDER ===
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
                    Debug.LogWarning($"No slider setting data found for ID: {id}", comp);
                }
            }

            // === SWITCH ===
            foreach (var comp in switchComponents)
            {
                if (comp == null) continue;

                var id = comp.GetSettingID();
                if (string.IsNullOrEmpty(id)) continue;

                var match = System.Array.Find(settingDataReference.switchSettingDataArray, s => s.ID == id);
                if (!string.IsNullOrEmpty(match.ID))
                {
                    comp.AssignData(match);
                }
                else
                {
                    Debug.LogWarning($"No switch setting data found for ID: {id}", comp);
                }
            }
        }

        [Button]
        private void FetchAllComponentsInChildren()
        {
            sliderComponents = GetComponentsInChildren<SliderSetting>(true);
            switchComponents = GetComponentsInChildren<SwitchSetting>(true);
        }
    }
}
