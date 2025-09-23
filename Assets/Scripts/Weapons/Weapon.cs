using UnityEngine;

namespace Iztar.ShipModule
{
    public abstract class Weapon : MonoBehaviour
    {
        public string ID;

        public Vector3 ShotDirection { get; set; }
        public bool WeaponActive { get; set; }
    }
}