using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace Iztar.ShipModule
{
    public class SpreadGunWeapon : Weapon
    {
        [Title("Weapon Firing")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint; // hanya 1 titik spawn
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private int projectileDamage = 8;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private int projectileCount = 5; // jumlah peluru per tembakan
        [SerializeField] private float spreadAngle = 30f; // total sudut sebaran (derajat)

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
            // Hitung offset sudut per peluru
            float angleStep = spreadAngle / (projectileCount - 1);
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                Projectile proj = projectilePool.Get();

                // Set posisi spawn
                proj.transform.position = firePoint.position;

                // Arah dasar dari firePoint
                Vector3 baseDir = new Vector3(firePoint.forward.x, 0f, firePoint.forward.z).normalized;

                // Rotasi sesuai offset spread
                float currentAngle = startAngle + (angleStep * i);
                Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.up);

                // Apply rotasi langsung ke projectile
                Vector3 shotDir = rot * baseDir;
                proj.transform.rotation = Quaternion.LookRotation(shotDir);

                // Set parameter projectile
                proj.speed = projectileSpeed;
                proj.damage = projectileDamage;

                // Karena spreading sudah diatur dari rotation, tinggal kasih Fire tanpa modif
                proj.Fire(proj.transform.forward);
            }
        }
    }
}