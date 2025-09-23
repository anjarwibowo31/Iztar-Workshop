using Cysharp.Threading.Tasks;
using Iztar.Manager;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button dashButton;

    void Start()
    {
        AssignButtonAsync().Forget();
    }

    private async UniTaskVoid AssignButtonAsync()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        await UniTask.WaitUntil(() => GameManager.Instance.ActiveShip != null);

        if (dashButton != null)
        {
            dashButton.onClick.AddListener(Dash);
        }
    }

    private void Dash()
    {
        //GameplayInputSystem.Instance?.OnDashPerformed();
    }
}
