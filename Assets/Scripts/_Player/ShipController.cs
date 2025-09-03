using Sirenix.OdinInspector;
using System.Threading;
using UnityEngine;

/// <summary> 
/// TODO: 
/// - Asteroid rotate 
/// </summary>

public class ShipController : MonoBehaviour
{
    [Title("Movement")]
    [SerializeField] private float maxMoveSpeed = 40f;
    [SerializeField] private float turnDrag = 12f;
    [SerializeField] private float turnDragThreshold = 45f;
    [SerializeField] private float inertiaSmooth = 0.3f;

    [Title("Rotation")]
    [SerializeField] private float maxAngularSpeed = 270f;
    [SerializeField] private float bankAngle = 60f;
    [SerializeField] private float bankLerpSpeed = 6f;

    [Title("Weapon")]
    [SerializeField] private Transform weaponVisual;
    [SerializeField] private float aimResetDelay = 1.5f; 
    [SerializeField] private float aimResetLerpSpeed = 4f; 

    [Title("Dash")]
    [SerializeField] private float dashSpeed = 100f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashVfxStopThreshold = 0.2f;

    [Title("Collision / Knockback")]
    [SerializeField] private float collisionFreezeTime = 0.5f;
    [SerializeField] private float collisionCooldown = 0.12f;
    [SerializeField] private float postCollisionIdleDelay = 0.5f;
    [SerializeField] private Vector3 maxKnockbackTilt = new Vector3(20f, 10f, 15f);
    [SerializeField] private float knockbackTiltLerpSpeed = 6f;
    [SerializeField] private float knockbackForceMultiplier = 0.5f;
    [SerializeField] private float knockbackDecaySpeed = 8f;

    [Title("Idle Bobbing")]
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobSmooth = 5f;

    [Title("Visual")]
    [SerializeField] private Transform shipVisual;
    [SerializeField] private ParticleSystem thrustVfx;
    [SerializeField] private ParticleSystem dashVfx;

    // ----- new tuning fields -----
    [SerializeField] private float knockbackTiltEndThreshold = 1f;      // degree magnitude under which we consider tilt "neutral"
    [SerializeField] private float inputCancelTiltThreshold = 8f;      // if player inputs and tilt <= this, cancel knockback early

    // Input
    private Vector2 moveInput;
    private Vector2 aimInput;
    private bool hasInput;
    private bool hasAimInput;

    // Speed/Rot
    private float currentSpeed;
    private float currentAngularSpeed;
    private float speedVelocity;
    private float angularVelocity;
    private Quaternion visualBaseLocalRot;

    // Visual
    private float targetYaw;
    private float currentBank;
    private float bobOffsetY;
    private Vector3 visualBaseLocalPos;

    // Weapon
    private float aimResetTimer;

    // Dash
    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing;
    private CancellationTokenSource dashCts;

    // Collision / State
    private float collisionFreezeTimer;
    private float collisionCooldownTimer;
    private float postCollisionIdleTimer;
    private bool isColliding;
    private bool vfxPlaying;

    // Knockback
    private Vector3 knockbackDir;
    private float knockbackForce;
    private bool isKnockback;
    private Vector3 knockbackTiltEuler;
    private Vector3 currentKnockbackTiltEuler;

    private void Awake()
    {
        if (thrustVfx != null) thrustVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dashVfx != null) dashVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        targetYaw = transform.eulerAngles.y;

        if (shipVisual != null)
        {
            visualBaseLocalPos = shipVisual.localPosition;
            visualBaseLocalRot = shipVisual.localRotation; // simpan rotasi dasar
        }

        hasInput = false;
        vfxPlaying = false;
        isDashing = false;
    }

    private void Update()
    {
        // timers
        if (collisionCooldownTimer > 0f) collisionCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

        // Knockback override (while active, we handle knockback; but we will allow early exit if conditions met)
        if (isKnockback)
        {
            HandleKnockback();
            return;
        }

        // Freeze setelah collision
        if (collisionFreezeTimer > 0f)
        {
            collisionFreezeTimer -= Time.deltaTime;
            if (collisionFreezeTimer <= 0f)
            {
                isColliding = false;
                currentSpeed = 0f;
                postCollisionIdleTimer = postCollisionIdleDelay;

                if (hasInput && thrustVfx != null && !vfxPlaying)
                {
                    thrustVfx.Play(true);
                    vfxPlaying = true;
                }
            }
        }
        else if (postCollisionIdleTimer > 0f)
        {
            postCollisionIdleTimer -= Time.deltaTime;
        }

        if (isColliding) return;

        // Input & Dash
        HandleMoveInput();
        HandleAimInput();
        HandleDashInput();

        // Normal ship logic
        HandleThrustVfx();
        HandleMovement();
        HandleRotation();
        HandleBanking();
        HandleVisuals();
        HandleDash();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionCooldownTimer > 0f || collisionFreezeTimer > 0f) return;

        isColliding = true;

        // -- knockback direction --
        knockbackDir = (transform.position - other.transform.position);
        knockbackDir.y = 0f;
        knockbackDir = knockbackDir.sqrMagnitude > 1e-4f ? knockbackDir.normalized : -transform.forward;

        // -- force: scale by current speed, tunable --
        knockbackForce = Mathf.Clamp(currentSpeed * knockbackForceMultiplier, 0f, 50f);

        // ----- Baseline tilt dari bank / current visual rotasi -----
        Vector3 baseTilt = new Vector3(
            shipVisual.localRotation.eulerAngles.x,
            shipVisual.localRotation.eulerAngles.y,
            shipVisual.localRotation.eulerAngles.z
        );

        // Konversi agar -180..180 (supaya nggak overflow 360)
        baseTilt.x = Mathf.DeltaAngle(0f, baseTilt.x);
        baseTilt.y = Mathf.DeltaAngle(0f, baseTilt.y);
        baseTilt.z = Mathf.DeltaAngle(0f, baseTilt.z);

        // ----- Knockback tilt dari arah relatif -----
        Vector3 localKnockback = transform.InverseTransformDirection(knockbackDir);

        float tiltX = Mathf.Clamp(-Mathf.Abs(localKnockback.z) * maxKnockbackTilt.x, -maxKnockbackTilt.x, 0f); // selalu negatif
        float tiltY = Mathf.Clamp(localKnockback.x * maxKnockbackTilt.y, -maxKnockbackTilt.y, maxKnockbackTilt.y);
        float tiltZ = Mathf.Clamp(-localKnockback.x * maxKnockbackTilt.z * 0.5f, -maxKnockbackTilt.z, maxKnockbackTilt.z);

        // ----- Gabungkan -----
        knockbackTiltEuler = baseTilt + new Vector3(tiltX, tiltY, tiltZ);

        currentKnockbackTiltEuler = Vector3.zero;

        isKnockback = true;
        currentSpeed = 0f;
        currentAngularSpeed = 0f;

        // Timers
        collisionFreezeTimer = collisionFreezeTime;
        collisionCooldownTimer = collisionCooldown;

        // Reset dash & VFX
        isDashing = false;
        dashTimer = 0f;
        if (dashVfx != null) dashVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (thrustVfx != null) thrustVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        vfxPlaying = false;
    }

    // ---------- Input ----------
    private void HandleMoveInput()
    {
        moveInput = InputManager.Instance != null ? InputManager.Instance.GetMoveInput() : Vector2.zero;
        hasInput = moveInput.sqrMagnitude > 0.01f;
    }
    
    private void HandleAimInput()
    {
        aimInput = InputManager.Instance != null ? InputManager.Instance.GetAimInput() : Vector2.zero;
        hasAimInput = aimInput.sqrMagnitude > 0.01f;

        if (weaponVisual == null) return;

        if (hasAimInput)
        {
            // reset timer saat ada input
            aimResetTimer = aimResetDelay;

            // arah dari input
            Vector3 aimDir = new Vector3(aimInput.x, 0f, aimInput.y).normalized;
            Quaternion targetRot = Quaternion.LookRotation(aimDir, Vector3.up);

            weaponVisual.rotation = Quaternion.Slerp(
                weaponVisual.rotation,
                targetRot,
                Time.deltaTime * 15f
            );
        }
        else
        {
            if (aimResetTimer > 0f)
            {
                // countdown delay
                aimResetTimer -= Time.deltaTime;
            }
            else
            {
                // balik ke arah ship facing
                Quaternion targetRot = Quaternion.LookRotation(transform.forward, Vector3.up);

                weaponVisual.rotation = Quaternion.Slerp(
                    weaponVisual.rotation,
                    targetRot,
                    Time.deltaTime * aimResetLerpSpeed
                );
            }
        }
    }

    private void HandleDashInput()
    {
        if (InputManager.Instance == null) return;

        // 1) Dash terjadi jika tombol ditekan, tidak sedang dash, dan cooldown dash telah selesai

        if (InputManager.Instance.ConsumeDashPressed() && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (dashVfx != null) dashVfx.Play(true);
        }
    }

    // ---------- Knockback ----------
    private void HandleKnockback()
    {
        // 1) apply movement + tilt while force remains
        if (knockbackForce > 0.05f)
        {
            // decay force smoothly
            knockbackForce = Mathf.Lerp(knockbackForce, 0f, Time.deltaTime * knockbackDecaySpeed);
            transform.position += knockbackDir * knockbackForce * Time.deltaTime;

            // tilt approaches target (if player gives input, approach a bit faster but not instant)
            float effectiveLerp = hasInput ? knockbackTiltLerpSpeed * 1.5f : knockbackTiltLerpSpeed;
            currentKnockbackTiltEuler = Vector3.Lerp(currentKnockbackTiltEuler, knockbackTiltEuler, Time.deltaTime * effectiveLerp);

            ApplyVisualTilt();
            return;
        }

        // 2) recovery: tilt lerp back to zero
        {
            // recovery speed slightly faster if player inputs, but tuned to avoid snap
            float effectiveLerp = hasInput ? knockbackTiltLerpSpeed * 1.5f : knockbackTiltLerpSpeed * 1.1f;
            currentKnockbackTiltEuler = Vector3.Lerp(currentKnockbackTiltEuler, Vector3.zero, Time.deltaTime * effectiveLerp);

            ApplyVisualTilt();

            // decide when to finish knockback and return control
            float tiltMag = currentKnockbackTiltEuler.magnitude;

            bool tiltCloseEnough = tiltMag <= knockbackTiltEndThreshold;
            bool playerWantsControl = hasInput && tiltMag <= inputCancelTiltThreshold;

            if (tiltCloseEnough || playerWantsControl)
            {
                // finish knockback
                isKnockback = false;
                knockbackForce = 0f;

                // clear freeze so player can move immediately
                isColliding = false;
                collisionFreezeTimer = 0f;
                postCollisionIdleTimer = 0f;

                // if player is holding input, (re)start thrust vfx
                if (hasInput && thrustVfx != null && !vfxPlaying)
                {
                    thrustVfx.Play(true);
                    vfxPlaying = true;
                }

                // process one immediate frame of movement so control feels instant
                HandleMoveInput();
                HandleThrustVfx();
                HandleMovement();
                HandleRotation();
                HandleBanking();
                HandleVisuals();
                HandleDash();
            }
        }
    }

    // ---------- Movement ----------
    private void HandleMovement()
    {
        float currentYaw = transform.eulerAngles.y;
        float yawDiff = Mathf.DeltaAngle(currentYaw, targetYaw);

        if (hasInput)
        {
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            float targetSpeed = maxMoveSpeed;
            if (Mathf.Abs(yawDiff) > turnDragThreshold)
                targetSpeed = Mathf.Max(0f, targetSpeed - turnDrag);

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, inertiaSmooth);
        }
        else
        {
            targetYaw = currentYaw;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref speedVelocity, inertiaSmooth);
        }

        transform.position += currentSpeed * Time.deltaTime * transform.forward;
    }

    private void HandleRotation()
    {
        float currentYaw = transform.eulerAngles.y;

        if (!hasInput)
        {
            targetYaw = currentYaw;
            currentAngularSpeed = 0f;
            return;
        }

        float yawDiff = Mathf.DeltaAngle(currentYaw, targetYaw);
        float targetAngular = Mathf.Abs(yawDiff) > 0.1f ? maxAngularSpeed : 0f;

        currentAngularSpeed = Mathf.SmoothDamp(currentAngularSpeed, targetAngular, ref angularVelocity, inertiaSmooth);
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, currentAngularSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    private void HandleBanking()
    {
        float yawDiff = Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw);
        float bankTarget = Mathf.Clamp(yawDiff / 45f, -1f, 1f) * -bankAngle;
        if (isDashing) bankTarget *= 1.5f;
        currentBank = Mathf.Lerp(currentBank, bankTarget, bankLerpSpeed * Time.deltaTime);
    }

    // ---------- Visuals ----------
    private void HandleVisuals()
    {
        if (shipVisual == null) return;

        ApplyVisualTilt();

        float idleFactor = 1f - Mathf.Clamp01(currentSpeed / maxMoveSpeed);
        float targetOffset = 0f;

        if (postCollisionIdleTimer <= 0f)
        {
            targetOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude * idleFactor;
            if (currentSpeed < 0.1f)
                targetOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        }

        bobOffsetY = Mathf.Lerp(bobOffsetY, targetOffset, Time.deltaTime * bobSmooth);
        shipVisual.localPosition = visualBaseLocalPos + Vector3.up * bobOffsetY;
    }

    private void ApplyVisualTilt()
    {
        if (shipVisual == null) return;

        Quaternion bankRot = Quaternion.Euler(0f, 0f, currentBank);
        Quaternion knockbackRot = Quaternion.Euler(currentKnockbackTiltEuler);

        // Selalu mulai dari rotasi dasar
        shipVisual.localRotation = visualBaseLocalRot * bankRot * knockbackRot;
    }

    // ---------- VFX ----------
    private void HandleThrustVfx()
    {
        if (thrustVfx == null) return;

        if (hasInput && !vfxPlaying)
        {
            thrustVfx.Play(true);
            vfxPlaying = true;
        }
        else if (!hasInput && vfxPlaying)
        {
            thrustVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            vfxPlaying = false;
        }
    }

    // ---------- Dash ----------
    private void HandleDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(1f - (dashTimer / Mathf.Max(0.0001f, dashDuration)));
        float dashCurve = Mathf.Sin(t * Mathf.PI);

        float targetSpeed = maxMoveSpeed + dashCurve * dashSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

        if (dashTimer <= 0f)
        {
            isDashing = false;
            if (dashVfx != null) dashVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else if (dashTimer <= dashVfxStopThreshold)
        {
            if (dashVfx != null) dashVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
