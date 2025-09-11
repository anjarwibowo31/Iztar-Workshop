using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float rotationLerpSpeed = 5f;

    private Transform player;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // biar smooth
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // jangan miring
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    private void OnEnable()
    {
        
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        // kalau terlalu dekat ? stop
        if (distance < stopDistance)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // --- Movement ---
        Vector3 direction = toPlayer.normalized;
        rb.linearVelocity = direction * speed;

        // --- Rotation ---
        Vector3 lookDir = rb.linearVelocity;
        lookDir.y = 0f; // tetap horizontal
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationLerpSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
