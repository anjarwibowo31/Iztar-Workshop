using Iztar.Manager;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Iztar.ShipModule
{
    public class ShipController : MonoBehaviour
    {
        public static ShipController Instance { get; private set; }
        public event Action<float> OnCollision;

#if UNITY_EDITOR // Debugger
        #region Debug Section (Odin)

        // Tampilkan debug hanya saat FoldoutGroup "Debug" dibuka
        [FoldoutGroup("Debug", Expanded = false), ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private float CurrentSpeed => currentSpeed;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private Vector3 CurrentVelocity => currentVelocity;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private bool IsDashing => isDashing;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private bool IsKnockback => isKnockback;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private bool IsColliding => isColliding;

        // Paksa inspector Odin refresh realtime saat Play Mode
        [OnInspectorGUI, PropertyOrder(-11)]
        private void ForceRepaint()
        {
            GUIHelper.RequestRepaint();
        }

        #endregion
#endif

        #region Inspector Fields

        [Title("Movement")]
        [SerializeField] private float maxMoveSpeed = 35f;
        [SerializeField] private float inertiaSmooth = 0.3f;
        [SerializeField] private float inertiaFollowStrength = 2f;

        [Title("Boost Start")]
        [SerializeField] private float boostStartThreshold = 5f;
        [SerializeField] private float boostStartMultiplier = 2f;

        [Title("Rotation")]
        [SerializeField] private float maxAngularSpeed = 200f;
        [SerializeField] private float bankAngle = 40f;
        [SerializeField] private float bankLerpSpeed = 4f;

        [Title("Dash")]
        [SerializeField] private float dashSpeed = 80f;
        [SerializeField] private float dashDuration = 0.4f;
        [SerializeField] private float dashCooldown = 1.8f;
        [SerializeField] private float dashVfxStopThreshold = 0.2f;

        [Title("Collision / Knockback")]
        [SerializeField] private int obstacleLayerIndex = 8;
        [SerializeField] private float collisionFreezeTime = 0.5f;
        [SerializeField] private float collisionCooldown = 0.2f;
        [SerializeField] private float postCollisionIdleDelay = 0.7f;
        [SerializeField] private Vector3 maxKnockbackTilt = new(30f, 20f, 25f);
        [SerializeField] private float knockbackTiltLerpSpeed = 4f;
        [SerializeField] private float knockbackForceMultiplier = 0.8f;
        [SerializeField] private float knockbackDecaySpeed = 6f;
        [SerializeField] private float knockbackTiltEndThreshold = 1f;
        [SerializeField] private float inputCancelTiltThreshold = 8f;

        [Title("Idle Bobbing")]
        [SerializeField] private float bobAmplitude = 0.18f;
        [SerializeField] private float bobFrequency = 1.5f;
        [SerializeField] private float bobSmooth = 5f;

        [Title("Visual")]
        [SerializeField] private Transform shipVisual;
        [SerializeField] private ParticleSystem thrustVfx;
        [SerializeField] private ParticleSystem dashVfx;
        [SerializeField] private ParticleSystem boostStartVfx;

        #endregion

        #region Private State

        private Vector2 moveInput;
        private bool hasInput;

        private Vector3 currentVelocity;
        private float currentSpeed;
        private float targetYaw;

        private float currentBank;
        private float bobOffsetY;
        private Quaternion visualBaseLocalRot;
        private Vector3 visualBaseLocalPos;

        private float dashTimer;
        private float dashCooldownTimer;
        private bool isDashing;

        private float collisionFreezeTimer;
        private float collisionCooldownTimer;
        private float postCollisionIdleTimer;
        private bool isColliding;
        private bool vfxPlaying;

        private Vector3 knockbackDir;
        private float knockbackForce;
        private bool isKnockback;
        private Vector3 knockbackTiltEuler;
        private Vector3 currentKnockbackTiltEuler;
        private Vector3 knockbackSpinAxis;
        private float knockbackSpinSpeed;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            StopVfx(thrustVfx, true);
            StopVfx(dashVfx, true);

            targetYaw = transform.eulerAngles.y;

            if (shipVisual != null)
            {
                visualBaseLocalPos = shipVisual.localPosition;
                visualBaseLocalRot = shipVisual.localRotation;
            }

            hasInput = false;
            vfxPlaying = false;
            isDashing = false;
        }

        private void Update()
        {
            UpdateTimers();

            if (isKnockback)
            {
                HandleKnockback();
                return;
            }

            if (isColliding) return;

            HandleMoveInput();
            HandleDashInput();

            HandleThrustVfx();
            HandleMovement();
            HandleBanking();
            HandleVisuals();
            HandleDash();
        }

        private void OnEnable()
        {
            StopVfx(dashVfx, true);
            StopVfx(thrustVfx, true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collisionCooldownTimer > 0f || collisionFreezeTimer > 0f) return;

            if (other.gameObject.layer == obstacleLayerIndex)
            {
                OnCollision?.Invoke(currentSpeed);
                BeginCollision(other);
            }
        }

        #endregion

        #region Input

        private void HandleMoveInput()
        {
            moveInput = InputManager.Instance != null ? InputManager.Instance.GetMoveInput() : Vector2.zero;
            hasInput = moveInput.sqrMagnitude > 0.01f;
        }

        private void HandleDashInput()
        {
            // BUAT SYSTEM DASH BARU DISINI, DRAINING DASH

            if (InputManager.Instance == null) return;

            if (InputManager.Instance.ConsumeDashPressed() && !isDashing && dashCooldownTimer <= 0f)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                if (dashVfx != null) dashVfx.Play(true);
            }
        }

        #endregion

        #region Movement with Inertia

        private void HandleMovement()
        {
            Vector3 targetVelocity = Vector3.zero;

            if (hasInput)
            {
                // Arah gerakan
                Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                if (dir != Vector3.zero)
                    targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                // Hitung target kecepatan
                float targetSpeed = maxMoveSpeed;

                // 🚀 Boost Start
                if (currentSpeed < boostStartThreshold)
                {
                    targetSpeed *= boostStartMultiplier;
                    boostStartVfx?.Play(true);
                }
                else
                {
                    boostStartVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                targetVelocity = transform.forward * targetSpeed;
            }

            // Lerp velocity (inertia)
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                Time.deltaTime * inertiaFollowStrength
            );

            // Selalu sinkron speed dengan velocity
            currentSpeed = currentVelocity.magnitude;

            // Rotasi menuju target arah
            Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                maxAngularSpeed * Time.deltaTime
            );

            // Update posisi
            transform.position += currentVelocity * Time.deltaTime;
        }

        private void HandleBanking()
        {
            float yawDiff = Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw);
            float bankTarget = Mathf.Clamp(yawDiff / 45f, -1f, 1f) * -bankAngle;
            if (isDashing) bankTarget *= 1.5f;
            currentBank = Mathf.Lerp(currentBank, bankTarget, bankLerpSpeed * Time.deltaTime);
        }

        #endregion

        #region Visuals

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

            shipVisual.localRotation = visualBaseLocalRot * bankRot * knockbackRot;
        }

        #endregion

        #region VFX

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
                StopVfx(thrustVfx);
                vfxPlaying = false;
            }
        }

        private void HandleDash()
        {
            if (!isDashing) return;

            dashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - dashTimer / Mathf.Max(0.0001f, dashDuration));
            float dashCurve = Mathf.Sin(t * Mathf.PI);

            float targetSpeed = maxMoveSpeed + dashCurve * dashSpeed;
            Vector3 dashVelocity = transform.forward * targetSpeed;

            currentVelocity = Vector3.Lerp(currentVelocity, dashVelocity, Time.deltaTime * 10f);

            currentSpeed = currentVelocity.magnitude;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                StopVfx(dashVfx, true);
            }
            else if (dashTimer <= dashVfxStopThreshold)
            {
                StopVfx(dashVfx);
            }
        }

        #endregion

        #region Knockback

        private void BeginCollision(Collider other)
        {
            isColliding = true;

            knockbackDir = transform.position - other.transform.position;
            knockbackDir.y = 0f;
            knockbackDir = knockbackDir.sqrMagnitude > 1e-4f ? knockbackDir.normalized : -transform.forward;

            float dashBonus = isDashing ? 2f : 1f;
            knockbackForce = Mathf.Clamp(currentSpeed * knockbackForceMultiplier * dashBonus, 0f, 80f);

            Vector3 localKnockback = transform.InverseTransformDirection(knockbackDir);
            float speedFactor = Mathf.Clamp01(currentSpeed / maxMoveSpeed);
            float tiltMultiplier = 1f + speedFactor * (isDashing ? 1.5f : 0.5f);

            float tiltX = Mathf.Clamp(-Mathf.Abs(localKnockback.z) * maxKnockbackTilt.x * tiltMultiplier, -maxKnockbackTilt.x * 2f, 0f);
            float tiltY = Mathf.Clamp(localKnockback.x * maxKnockbackTilt.y * tiltMultiplier, -maxKnockbackTilt.y * 2f, maxKnockbackTilt.y * 2f);
            float tiltZ = Mathf.Clamp(-localKnockback.x * maxKnockbackTilt.z * 0.5f * tiltMultiplier, -maxKnockbackTilt.z * 2f, maxKnockbackTilt.z * 2f);

            knockbackTiltEuler = new Vector3(tiltX, tiltY, tiltZ);
            currentKnockbackTiltEuler = Vector3.zero;

            isKnockback = true;

            // Reset velocity
            currentVelocity = Vector3.zero;
            currentSpeed = 0f;

            collisionFreezeTimer = collisionFreezeTime;
            collisionCooldownTimer = collisionCooldown;

            isDashing = false;
            dashTimer = 0f;
            StopVfx(dashVfx);
            StopVfx(thrustVfx);
            vfxPlaying = false;

            knockbackSpinAxis = (transform.position - other.transform.position).normalized;
            if (knockbackSpinAxis == Vector3.zero)
                knockbackSpinAxis = Random.onUnitSphere;

            knockbackSpinSpeed = currentSpeed * 5f * dashBonus;
        }

        private void HandleKnockback()
        {
            if (knockbackForce > 0.05f)
                ApplyKnockbackMovement();
            else
                RecoverFromKnockback();
        }

        private void ApplyKnockbackMovement()
        {
            float effectiveDecay = isDashing ? knockbackDecaySpeed * 0.5f : knockbackDecaySpeed;
            knockbackForce = Mathf.Lerp(knockbackForce, 0f, Time.deltaTime * effectiveDecay);
            transform.position += knockbackDir * knockbackForce * Time.deltaTime;

            float effectiveLerp = hasInput ? knockbackTiltLerpSpeed * 1.5f : knockbackTiltLerpSpeed;
            currentKnockbackTiltEuler = Vector3.Lerp(currentKnockbackTiltEuler, knockbackTiltEuler, Time.deltaTime * effectiveLerp);

            ApplyVisualTilt();
            ApplyKnockbackSpin();
        }

        private void RecoverFromKnockback()
        {
            float effectiveLerp = hasInput ? knockbackTiltLerpSpeed * 1.5f : knockbackTiltLerpSpeed * 1.1f;
            currentKnockbackTiltEuler = Vector3.Lerp(currentKnockbackTiltEuler, Vector3.zero, Time.deltaTime * effectiveLerp);

            ApplyVisualTilt();
            ApplyKnockbackSpin();

            float tiltMag = currentKnockbackTiltEuler.magnitude;
            bool tiltCloseEnough = tiltMag <= knockbackTiltEndThreshold;
            bool playerWantsControl = hasInput && tiltMag <= inputCancelTiltThreshold;

            if (tiltCloseEnough || playerWantsControl)
                EndKnockback();
        }

        private void ApplyKnockbackSpin()
        {
            if (knockbackSpinSpeed > 0.1f)
            {
                transform.Rotate(knockbackSpinAxis * knockbackSpinSpeed * Time.deltaTime, Space.Self);
                knockbackSpinSpeed = Mathf.Lerp(knockbackSpinSpeed, 0f, Time.deltaTime * 2f);
            }
        }

        private void EndKnockback()
        {
            isKnockback = false;
            knockbackForce = 0f;
            knockbackSpinSpeed = 0f;

            isColliding = false;
            collisionFreezeTimer = 0f;
            postCollisionIdleTimer = 0f;

            currentKnockbackTiltEuler = Vector3.zero;

            if (shipVisual != null)
                shipVisual.localRotation = visualBaseLocalRot;

            if (hasInput && thrustVfx != null && !vfxPlaying)
            {
                thrustVfx.Play(true);
                vfxPlaying = true;
            }
        }


        #endregion

        #region Helpers

        private void UpdateTimers()
        {
            if (collisionCooldownTimer > 0f) collisionCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
            if (collisionFreezeTimer > 0f)
            {
                collisionFreezeTimer -= Time.deltaTime;
                if (collisionFreezeTimer <= 0f)
                {
                    isColliding = false;
                    currentSpeed = 0f;
                    postCollisionIdleTimer = postCollisionIdleDelay;
                }
            }
            else if (postCollisionIdleTimer > 0f)
            {
                postCollisionIdleTimer -= Time.deltaTime;
            }
        }

        private void StopVfx(ParticleSystem vfx, bool clear = false)
        {
            if (vfx == null) return;
            vfx.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }

        #endregion
    }
}
