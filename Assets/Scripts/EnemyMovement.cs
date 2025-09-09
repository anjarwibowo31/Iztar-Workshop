using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float avoidForce = 5f;
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private float stopDistance = 0.5f; // jarak aman ke player
    [SerializeField] private LayerMask obstacleMask;

    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Hapus velocity biar tidak ada efek dorong/mundur
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        // Kalau terlalu dekat, jangan maju lagi
        if (distance < stopDistance) return;

        Vector3 direction = toPlayer.normalized;

        // --- Obstacle avoidance ---
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance, obstacleMask))
        {
            Vector3 perp = Vector3.Cross(Vector3.up, direction).normalized;
            if (Physics.Raycast(transform.position, perp, rayDistance, obstacleMask))
            {
                perp = -perp;
            }
            direction += perp * avoidForce;
            direction.Normalize();
        }

        // --- Movement ---
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // --- Rotation ---
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.2f));
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Vector3 dir = (player.position - transform.position).normalized;
        Gizmos.DrawLine(transform.position, transform.position + dir * rayDistance);
    }
}
