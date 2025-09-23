using Cysharp.Threading.Tasks;
using Iztar.ShipModule;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ShipController ActiveShip { get; set; }

    [SerializeField] private ShipController shipPrefab;
    [SerializeField] private bool selfInitializeOnAwake = false;

    public bool IsPaused { get; private set; } = false;

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
        ActiveShip.SetUpFromSettingData();

        CameraManager.Instance.CinemachineCamera.Follow = ActiveShip.transform;
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    // === PAUSE HANDLING ===
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        StartCoroutine(PauseCoroutine());
    }

    private IEnumerator PauseCoroutine()
    {
        IsPaused = true;

        Debug.Log("Game Paused");

        yield return new WaitForSecondsRealtime(0.4f);

        Debug.Log("Pause menu animasi selesai");

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        Time.timeScale = 1f;
        IsPaused = false;

        Debug.Log("Game Resumed");
    }
}
