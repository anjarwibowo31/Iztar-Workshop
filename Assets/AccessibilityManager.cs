using System;
using UnityEngine;

namespace Iztar.Manager
{
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }
        public event Action<bool> OnConitionChanged;


        public bool MotionSicknessReduction { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SaveDataManager.Instance.OnDataChanged += () =>
            {
                MotionSicknessReduction = SaveDataManager.Instance.SwitchSettingsDataDict["MotionSicknessReduction"].currentValue;
                OnConitionChanged?.Invoke(MotionSicknessReduction);
            };

            MotionSicknessReduction = SaveDataManager.Instance.SwitchSettingsDataDict["MotionSicknessReduction"].currentValue;
            OnConitionChanged?.Invoke(MotionSicknessReduction);
        }

        private void OnDestroy()
        {
            
        }
    }
}