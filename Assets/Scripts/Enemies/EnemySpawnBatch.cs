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

    private List<GameObject> spawnedEnemies = new();

    private IEnumerator SpawnBatchCoroutine()
    {
        spawnedEnemies.Clear();

        for (int i = 0; i < batchCount; i++)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new(
                transform.position.x + circle.x,
                0f,
                transform.position.z + circle.y
            );

            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity, SceneGameObjectContainer.EnemyContainer);
            enemy.SetActive(false);
            spawnedEnemies.Add(enemy);

            yield return new WaitForSeconds(spawnDelay);
        }

        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }
    }

    public void TriggerSpawn()
    {
        StartCoroutine(SpawnBatchCoroutine());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0f, transform.position.z), spawnRadius);
    }
}
