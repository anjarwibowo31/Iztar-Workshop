using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "ScriptableObjects/ShipData", order = 1)]
public class ShipDataSO : ScriptableObject
{
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
    [SerializeField] private float dashDuration = 2f;
    [SerializeField] private float dashCooldown = 4f;
    [SerializeField] private float dashVfxStopThreshold = 0.2f;
}
