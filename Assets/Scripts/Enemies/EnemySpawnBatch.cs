using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnBatch : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int batchCount = 5;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnDelay = 0.2f;

    private readonly List<GameObject> spawnedEnemies = new();

    /// <summary>
    /// Set jumlah musuh per batch dan jeda antar spawn
    /// </summary>
    public void SetData(int totalEnemyPerBatch)
    {
        batchCount = Mathf.Max(1, totalEnemyPerBatch);
    }

    /// <summary>
    /// Coroutine untuk spawn musuh satu per satu
    /// </summary>
    private IEnumerator SpawnBatchCoroutine()
    {
        spawnedEnemies.Clear();

        for (int i = 0; i < batchCount; i++)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab belum di-assign!");
                yield break;
            }

            // Tentukan posisi random dalam lingkaran
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new(
                transform.position.x + circle.x,
                0f,
                transform.position.z + circle.y
            );

            // Spawn enemy
            Transform parent = SceneGameObjectContainer.EnemyContainer != null
                ? SceneGameObjectContainer.EnemyContainer
                : null;

            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity, parent);
            spawnedEnemies.Add(enemy);

            // tunggu sebelum spawn berikutnya
            if (spawnDelay > 0f)
                yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Trigger spawn batch baru
    /// </summary>
    public void TriggerSpawn()
    {
        StopAllCoroutines(); // biar tidak overlap kalau dipanggil berulang
        StartCoroutine(SpawnBatchCoroutine());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0f, transform.position.z), spawnRadius);
    }
}
