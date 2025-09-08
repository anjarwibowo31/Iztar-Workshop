using Iztar.Manager;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Iztar.ShipModule
{
    public class ShipController : MonoBehaviour
    {
        public static ShipController Instance { get; private set; }

        public event Action<float> OnCollision;

        #region Inspector Fields

        [Title("Movement")]
        [SerializeField] private float maxMoveSpeed = 40f;
        [SerializeField] private float turnDrag = 12f;
        [SerializeField] private float turnDragThreshold = 45f;
        [SerializeField] private float inertiaSmooth = 0.3f;

        [Title("Rotation")]
        [SerializeField] private float maxAngularSpeed = 270f;
        [SerializeField] private float bankAngle = 60f;
        [SerializeField] private float bankLerpSpeed = 6f;

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
        [SerializeField] private float knockbackTiltEndThreshold = 1f;
        [SerializeField] private float inputCancelTiltThreshold = 8f;

        [Title("Idle Bobbing")]
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float bobSmooth = 5f;

        [Title("Visual")]
        [SerializeField] private Transform shipVisual;
        [SerializeField] private ParticleSystem thrustVfx;
        [SerializeField] private ParticleSystem dashVfx;

        #endregion

        #region Private State

        // Input
        private Vector2 moveInput;
        private bool hasInput;

        // Speed/Rotation
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

        // Dash
        private float dashTimer;
        private float dashCooldownTimer;
        private bool isDashing;

        // Collision
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
            HandleRotation();
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

            OnCollision?.Invoke(currentSpeed);
            BeginCollision(other);
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
            currentSpeed = 0f;
            currentAngularSpeed = 0f;

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

            // Reset tilt sepenuhnya
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
                postCollisionIdleTimer -= Time.deltaTime;
        }

        private void StopVfx(ParticleSystem vfx, bool clear = false)
        {
            if (vfx == null) return;
            vfx.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
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

        #region Movement

        private void HandleMovement()
        {
            float currentYaw = transform.eulerAngles.y;
            float yawDiff = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (hasInput)
            {
                Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                float targetSpeed = Mathf.Abs(yawDiff) > turnDragThreshold ? Mathf.Max(0f, maxMoveSpeed - turnDrag) : maxMoveSpeed;
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
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

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
    }
}