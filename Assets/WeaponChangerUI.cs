using UnityEngine;

public class WeaponChangerUI : MonoBehaviour
{
    public void ChangeWeapon()
    {
        if (GameManager.Instance.ActiveShip != null)
        {
            GameManager.Instance.ActiveShip.GetComponent<ShipWeaponController>().ChangeWeapon();
        }
    }
}
