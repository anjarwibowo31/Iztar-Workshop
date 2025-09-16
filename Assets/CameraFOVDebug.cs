using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFOVDebug : MonoBehaviour
{
    [SerializeField] private CameraManager targetCamera;
    [SerializeField] private TextMeshProUGUI debugText;

    public void UpdateFOVText(int value)
    {
        debugText.text = "Camera FOV: " + value;
    }
}
