using UnityEngine;

namespace Iztar.Manager
{
    public class GlobalManager : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}