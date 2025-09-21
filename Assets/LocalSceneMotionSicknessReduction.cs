using UnityEngine;
using Iztar.Manager;
using System;
using Cysharp.Threading.Tasks;

public class LocalSceneMotionSicknessReduction : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjects;

    private void Start()
    {
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        await UniTask.WaitUntil(() => AccessibilityManager.Instance != null);

        bool condition = AccessibilityManager.Instance.MotionSicknessReduction;

        foreach (var obj in gameObjects)
        {
            obj.SetActive(!condition);
        }

        AccessibilityManager.Instance.OnConitionChanged += (newCondition) =>
        {
            foreach (var obj in gameObjects)
            {
                obj.SetActive(!newCondition);
            }
        };
    }
}
