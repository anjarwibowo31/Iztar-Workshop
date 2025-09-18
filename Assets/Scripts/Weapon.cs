using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Vector3 ShotDirection { get; set; }
    public bool WeaponActive { get; set; }
}
