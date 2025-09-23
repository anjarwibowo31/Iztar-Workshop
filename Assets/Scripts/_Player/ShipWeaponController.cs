using UnityEngine;
using Sirenix.OdinInspector;
using Iztar.Manager;
using Iztar.Utility;
using Iztar.ShipModule;
using System.Collections.Generic;

public class ShipWeaponController : MonoBehaviour
{

    [Title("Weapon")]
    [SerializeField] private Weapon[] mainWeaponData;
    [SerializeField] private float aimResetDelay = 1.5f;
    [SerializeField] private float aimResetLerpSpeed = 4f;

    [Title("Visual")]
    [SerializeField] private Transform weaponDirectionalGuide;
    [SerializeField] private SpriteRenderer weaponDirectionMark;
    [SerializeField] private SpriteRenderer weaponActiveMark;
    [SerializeField] private ObjectRotator weaponActiveMarkRotator;
    [SerializeField] private Color aimActiveColor = Color.white;
    [SerializeField] private Color aimPassiveColor = Color.white;
    [SerializeField] private Color aimHasTargetColor = Color.red;
    [SerializeField] private float colorLerpSpeed = 10f;

    [Title("Target Locking")]
    [SerializeField] private float lockRange = 20f;
    [SerializeField, Range(1f, 90f)] private float lockAngle = 30f;
    [SerializeField, Range(1f, 120f)] private float lockAngleExit = 45f;
    [SerializeField] private LayerMask targetMask;

    private Weapon activeWeapon;
    private int weaponActiveIndex;
    private Vector2 aimInput;
    private bool hasAimInput;
    private bool hasTarget;
    private bool isReturningToForward;
    private Transform currentTarget;
    private float aimResetTimer;

    private void Awake()
    {
        foreach (var weapon in mainWeaponData)
        {
            weapon.gameObject.SetActive(false);
            weapon.WeaponActive = false;
        }

        activeWeapon = mainWeaponData[0];
        weaponActiveIndex = 0;

        activeWeapon.gameObject.SetActive(true);
        activeWeapon.WeaponActive = true;
    }

    private void Update()
    {
        UpdateAimInput();
        HandleAiming();

        HandleWeaponDashState();
    }

    [Button]
    public void ChangeWeapon()
    {
        activeWeapon.gameObject.SetActive(false);
        activeWeapon.WeaponActive = false;

        int nextIndex = weaponActiveIndex + 1;

        if (nextIndex >= mainWeaponData.Length)
        {
            nextIndex = 0;
        }

        weaponActiveIndex = nextIndex;
        activeWeapon = mainWeaponData[nextIndex];

        activeWeapon.gameObject.SetActive(true);
        activeWeapon.WeaponActive = true;
    }

    public void DisableWeapon()
    {
        weaponDirectionalGuide.gameObject.SetActive(false);
        weaponDirectionMark.gameObject.SetActive(false);
        weaponActiveMark.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void EnableWeapon()
    {
        weaponDirectionalGuide.gameObject.SetActive(true);
        weaponDirectionMark.gameObject.SetActive(true);
        weaponActiveMark.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    private void HandleWeaponDashState()
    {
        if (GameManager.Instance.ActiveShip.GetDashState() == true)
        {
            activeWeapon.WeaponActive = false;
        }
        else
        {
            activeWeapon.WeaponActive = true;
        }
    }

    private void UpdateAimInput()
    {
        aimInput = GameplayInputSystem.Instance.GetAimInput();
        hasAimInput = aimInput.sqrMagnitude > 0.01f;

        if (hasAimInput)
            isReturningToForward = false;
    }

    private void HandleAiming()
    {
        activeWeapon.ShotDirection = weaponDirectionalGuide.forward;

        if (currentTarget != null)
        {
            UpdateTargetLock();
        }
        else if (!isReturningToForward)
        {
            TryAcquireTarget();
        }

        if (hasTarget && currentTarget != null)
        {
            AimAtTarget();
        }
        else
        {
            HandleFreeAim();
        }
    }

    private void UpdateTargetLock()
    {
        Vector3 dirToTarget = GetFlatDirection(currentTarget.position - transform.position);
        float angleToTarget = GetAngleToTarget(dirToTarget);
        float dist = Vector3.Distance(transform.position, currentTarget.position);

        bool isWithinExitAngle = angleToTarget <= lockAngleExit;
        bool isWithinRange = dist <= lockRange;

        hasTarget = isWithinExitAngle && isWithinRange;

        if (!hasTarget)
        {
            currentTarget = null;
        }

        weaponActiveMarkRotator.enabled = true;
    }

    private void TryAcquireTarget()
    {
        weaponActiveMarkRotator.enabled = false;
        weaponActiveMarkRotator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        currentTarget = FindTargetInCone();
        hasTarget = currentTarget != null;
    }

    private void AimAtTarget()
    {
        LerpColors(aimHasTargetColor);

        Vector3 targetDir = GetFlatDirection(currentTarget.position - weaponDirectionalGuide.position);
        if (targetDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
        weaponDirectionalGuide.rotation = Quaternion.Slerp(
            weaponDirectionalGuide.rotation, targetRot, Time.deltaTime * 15f);
    }

    private void HandleFreeAim()
    {
        if (hasAimInput)
        {
            HandleActiveAim();
        }
        else
        {
            HandlePassiveAim();
        }
    }

    private void HandleActiveAim()
    {
        LerpColors(aimActiveColor, aimActiveColor);
        aimResetTimer = aimResetDelay;

        Vector3 aimDir = new Vector3(aimInput.x, 0f, aimInput.y).normalized;
        RotateWeapon(aimDir, 15f);
    }

    private void HandlePassiveAim()
    {
        LerpColors(aimPassiveColor, aimActiveColor);

        if (aimResetTimer > 0f)
        {
            aimResetTimer -= Time.deltaTime;
        }
        else
        {
            ResetToForward();
        }
    }

    private void LerpColors(Color activeColor)
    {
        LerpColors(activeColor, activeColor);
    }

    private void LerpColors(Color activeColor, Color directionColor)
    {
        weaponActiveMark.color = Color.Lerp(weaponActiveMark.color, activeColor, Time.deltaTime * colorLerpSpeed);
        weaponDirectionMark.color = Color.Lerp(weaponDirectionMark.color, directionColor, Time.deltaTime * colorLerpSpeed);
    }

    private void RotateWeapon(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        weaponDirectionalGuide.rotation = Quaternion.Slerp(
            weaponDirectionalGuide.rotation, targetRot, Time.deltaTime * speed);
    }

    private void ResetToForward()
    {
        isReturningToForward = true;
        currentTarget = null;
        hasTarget = false;

        Quaternion targetRot = Quaternion.LookRotation(transform.forward, Vector3.up);
        weaponDirectionalGuide.rotation = Quaternion.Slerp(
            weaponDirectionalGuide.rotation, targetRot, Time.deltaTime * aimResetLerpSpeed);

        if (Quaternion.Angle(weaponDirectionalGuide.rotation, targetRot) < 1f)
        {
            weaponDirectionalGuide.rotation = targetRot;
            isReturningToForward = false;
        }
    }

    private float GetAngleToTarget(Vector3 dirToTarget)
    {
        Vector3 referenceDir = hasAimInput ?
            new Vector3(aimInput.x, 0f, aimInput.y).normalized :
            weaponDirectionalGuide.forward;

        return Vector3.Angle(referenceDir, dirToTarget);
    }

    private Vector3 GetFlatDirection(Vector3 dir)
    {
        dir.y = 0f;
        return dir.normalized;
    }

    private Transform FindTargetInCone()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lockRange, targetMask);
        Vector3 baseDir = hasAimInput ?
            new Vector3(aimInput.x, 0f, aimInput.y).normalized :
            weaponDirectionalGuide.forward;

        Transform bestTarget = FindBestTarget(hits, baseDir, lockAngle);

        if (bestTarget == null && hasAimInput)
        {
            bestTarget = FindBestTarget(hits, weaponDirectionalGuide.forward, lockAngle);
        }

        return bestTarget;
    }

    private Transform FindBestTarget(Collider[] hits, Vector3 baseDir, float maxAngle)
    {
        Transform bestTarget = null;
        float closestAngle = maxAngle;

        foreach (Collider hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist > lockRange) continue;

            Vector3 dirToTarget = GetFlatDirection(hit.transform.position - transform.position);
            float angle = Vector3.Angle(baseDir, dirToTarget);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (weaponDirectionalGuide == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRange);

        Vector3 baseDir = hasAimInput ?
            new Vector3(aimInput.x, 0f, aimInput.y).normalized :
            weaponDirectionalGuide.forward;

        Gizmos.color = Color.green;
        DrawCone(weaponDirectionalGuide.position, baseDir, lockAngle, lockRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        DrawCone(weaponDirectionalGuide.position, baseDir, lockAngleExit, lockRange);

        Debug.DrawRay(weaponDirectionalGuide.position,
            new Vector3(aimInput.x, 0f, aimInput.y).normalized * 5f, Color.cyan);
    }

    private void DrawCone(Vector3 pos, Vector3 dir, float angle, float length)
    {
        int segments = 32;
        Quaternion rot = Quaternion.LookRotation(dir);
        Vector3 forward = rot * Vector3.forward;
        float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * length;

        Vector3 prevPoint = pos + forward * length + rot * Vector3.right * radius;

        for (int i = 1; i <= segments; i++)
        {
            float yaw = i * (360f / segments);
            Vector3 circleDir = Quaternion.AngleAxis(yaw, Vector3.forward) * Vector3.right;
            Vector3 nextPoint = pos + forward * length + rot * circleDir * radius;

            Gizmos.DrawLine(pos, nextPoint);
            Gizmos.DrawLine(prevPoint, nextPoint);

            prevPoint = nextPoint;
        }
    }
#endif
}