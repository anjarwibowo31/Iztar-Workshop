using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

public class TwinGunWeapon : Weapon
{
    [Title("Weapon Firing")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform[] firePoints; // ← array untuk 2 firepoint
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private float projectileSpeed = 20f;

    private float fireCooldown;
    private ObjectPool<Projectile> projectilePool;

    private void Awake()
    {
        projectilePool = new ObjectPool<Projectile>(
            createFunc: () =>
            {
                Projectile proj = Instantiate(projectilePrefab);

                proj.transform.parent = SceneGameObjectContainer.ProjectileContainer;
                proj.SetPool(projectilePool);
                return proj;
            },
            actionOnGet: proj =>
            {
                proj.gameObject.SetActive(true);
            },
            actionOnRelease: proj =>
            {
                proj.gameObject.SetActive(false);
            },
            actionOnDestroy: proj =>
            {
                Destroy(proj.gameObject);
            }
        );

        // Prevent immediate firing
        fireCooldown = fireRate;
    }

    private void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (!WeaponActive) return;

        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
            return;
        }

        FireProjectiles();
        fireCooldown = fireRate;
    }

    private void FireProjectiles()
    {
        foreach (Transform point in firePoints)
        {
            Projectile proj = projectilePool.Get();

            // Pastikan spawn di firePoint masing-masing
            proj.transform.position = point.position;
            proj.transform.rotation = point.rotation;

            // arah tembakan (tanpa Y)
            Vector3 shotDir = new Vector3(point.forward.x, 0f, point.forward.z);

            proj.speed = projectileSpeed;
            proj.damage = projectileDamage;
            proj.Fire(shotDir);
        }
    }
}
