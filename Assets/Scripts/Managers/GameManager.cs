using Cysharp.Threading.Tasks;
using Iztar.ShipModule;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ShipController ActiveShip { get; set; }

    [SerializeField] private ShipController shipPrefab;
    [SerializeField] private bool selfInitializeOnAwake = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (selfInitializeOnAwake)
        {
            SetupGameplay().Forget();
        }
    }

    [Button]
    private void SelfInitialize()
    {
        SetupGameplay().Forget();
    }


    public void StartGame()
    {
        LoadScene("Gameplay");

        SetupGameplay().Forget();
    }

    private async UniTaskVoid SetupGameplay()
    {
        await UniTask.WaitUntil(() => SceneGameObjectContainer.Instance != null);
        await UniTask.WaitUntil(() => CameraManager.Instance != null);

        if (ActiveShip == null)
        {
            ActiveShip = Instantiate(shipPrefab);
            ActiveShip.gameObject.SetActive(false);

            DontDestroyOnLoad(ActiveShip.gameObject);
        }

        ActiveShip.transform.SetParent(SceneGameObjectContainer.ShipContainer);
        ActiveShip.gameObject.SetActive(true);
        ActiveShip.SetUpFromSettingData(SaveDataManager.Instance.SettingsData);

        CameraManager.Instance.CinemachineCamera.Follow = ActiveShip.transform;
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}
