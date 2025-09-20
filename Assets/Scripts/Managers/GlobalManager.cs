using UnityEngine;

namespace Iztar.Manager
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != this && Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}