using Cysharp.Threading.Tasks;
using Iztar.Manager;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float rotationLerpSpeed = 5f;

    [Header("Boost")]
    [SerializeField] private float boostDistance = 15f;   // jarak mulai boost
    [SerializeField] private float boostMultiplier = 2f; // multiplier boost
    [SerializeField] private float speedLerpRate = 3f;   // seberapa cepat transisi boost <-> normal

    private Transform player;
    private Rigidbody rb;

    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        currentSpeed = speed; // mulai dengan normal speed
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.ActiveShip != null);
        player = GameManager.Instance.ActiveShip.transform;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        // stop kalau terlalu dekat
        if (distance < stopDistance)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // tentukan target speed
        float targetSpeed = (distance > boostDistance) ? speed * boostMultiplier : speed;

        // lerp ke target speed biar halus
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedLerpRate * Time.fixedDeltaTime);

        // movement
        Vector3 direction = toPlayer.normalized;
        rb.linearVelocity = direction * currentSpeed;

        // rotation
        Vector3 lookDir = rb.linearVelocity;
        lookDir.y = 0f;
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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, boostDistance);
    }
}
