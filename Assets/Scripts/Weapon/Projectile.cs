using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public int damage = 10;
    public GameObject hitEffect;

    private Vector3 direction;
    private float timer;

    private ObjectPool<Projectile> pool;

    private void Update()
    {
        transform.position += speed * Time.deltaTime * direction;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReleaseToPool();
        }
    }

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        ReleaseToPool();
    }

    public void SetPool(ObjectPool<Projectile> pool)
    {
        this.pool = pool;
    }

    public void Fire(Vector3 dir)
    {
        dir.y = 0f;
        direction = dir.normalized;
        transform.forward = direction;
    }

    private void ReleaseToPool()
    {
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            // fallback kalau pool belum diset
            Destroy(gameObject);
        }
    }
}
