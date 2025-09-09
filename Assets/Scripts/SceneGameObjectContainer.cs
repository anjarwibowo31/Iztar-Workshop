using UnityEngine;

public class SceneGameObjectContainer : MonoBehaviour
{
    public static SceneGameObjectContainer Instance { get; private set; }

    [SerializeField] private Transform shipContainer;
    [SerializeField] private Transform projectileContainer;
    [SerializeField] private Transform enemyContainer;

    public static Transform ShipContainer =>
        Instance != null && Instance.shipContainer != null ? Instance.shipContainer : null;

    public static Transform ProjectileContainer =>
        Instance != null && Instance.projectileContainer != null ? Instance.projectileContainer : null;

    public static Transform EnemyContainer =>
        Instance != null && Instance.enemyContainer != null ? Instance.enemyContainer : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
}
