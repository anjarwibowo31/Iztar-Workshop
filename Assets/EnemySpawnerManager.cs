using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject prefab;

    [Header("Spawn Settings")]
    [SerializeField] private float offsetRadius = 5f;   // jarak dari batas frustum
    [SerializeField] private float planeY = 0f;         // tinggi plane
    [SerializeField] private int maxAttempts = 20;

    [Header("Spawn Routine")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float initialDelay = 2f;
    private float spawnTimer;

    private Vector3[] frustumPolygon = new Vector3[0];
    private Vector3 lastSpawnPos;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        spawnTimer = initialDelay;
    }

    private void Update()
    {
        if (prefab == null || cam == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            frustumPolygon = GetFrustumPolygon(cam, planeY);
            if (frustumPolygon.Length >= 3)
            {
                Vector3 pos = GetSpawnOutsideFrustum(frustumPolygon, offsetRadius);
                lastSpawnPos = pos;
                Instantiate(prefab, pos, Quaternion.identity);
            }

            spawnTimer = spawnInterval;
        }
    }

    private Vector3[] GetFrustumPolygon(Camera cam, float planeY)
    {
        Plane p = new Plane(Vector3.up, new Vector3(0, planeY, 0));

        Vector2[] corners = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        List<Vector3> hits = new List<Vector3>();
        foreach (var c in corners)
        {
            Ray r = cam.ViewportPointToRay(new Vector3(c.x, c.y, 0f));
            if (p.Raycast(r, out float enter))
                hits.Add(r.GetPoint(enter));
        }

        if (hits.Count < 3) return new Vector3[0];

        // urutkan CCW
        Vector3 center = Vector3.zero;
        foreach (var h in hits) center += h;
        center /= hits.Count;

        hits.Sort((a, b) =>
        {
            float angA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angA.CompareTo(angB);
        });

        return hits.ToArray();
    }

    private Vector3 GetSpawnOutsideFrustum(Vector3[] polygon, float radius)
    {
        // pilih sisi random dari frustum polygon
        int idx = Random.Range(0, polygon.Length);
        Vector3 a = polygon[idx];
        Vector3 b = polygon[(idx + 1) % polygon.Length];

        // titik random di sisi itu
        float t = Random.Range(0f, 1f);
        Vector3 pointOnEdge = Vector3.Lerp(a, b, t);

        // arah keluar = normal sisi (2D XZ)
        Vector2 edgeDir = new Vector2(b.x - a.x, b.z - a.z).normalized;
        Vector2 outward = new Vector2(-edgeDir.y, edgeDir.x); // rotasi 90 derajat
        // pastikan outward benar-benar keluar (cek dengan center polygon)
        Vector3 polyCenter = Vector3.zero;
        foreach (var v in polygon) polyCenter += v;
        polyCenter /= polygon.Length;

        Vector2 toCenter = new Vector2(polyCenter.x - pointOnEdge.x, polyCenter.z - pointOnEdge.z);
        if (Vector2.Dot(outward, toCenter) > 0)
            outward = -outward;

        // spawn point = titik di edge + outward * radius
        Vector3 spawn = new Vector3(
            pointOnEdge.x + outward.x * radius,
            planeY,
            pointOnEdge.z + outward.y * radius
        );

        return spawn;
    }

    // Gizmos untuk debug
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (frustumPolygon != null && frustumPolygon.Length >= 3)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < frustumPolygon.Length; i++)
            {
                Vector3 p1 = frustumPolygon[i];
                Vector3 p2 = frustumPolygon[(i + 1) % frustumPolygon.Length];
                Gizmos.DrawLine(p1, p2);
            }
        }

        if (lastSpawnPos != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastSpawnPos, 0.3f);
        }
    }
#endif
}
