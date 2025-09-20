using System;
using UnityEngine;
using Unity.Cinemachine;
using Sirenix.OdinInspector;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Title("Camera Settings")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private int maxFOV = 58;
    [SerializeField] private int minFOV = 33;
    [SerializeField] private int zoomStep = 5;
    [SerializeField] private int defaultFOV = 48;

    [SerializeField] private CameraFOVDebug cameraFOVDebug;

    public CinemachineCamera CinemachineCamera => cinemachineCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cinemachineCamera.Lens.FieldOfView = defaultFOV;

        cameraFOVDebug?.UpdateFOVText((int)cinemachineCamera.Lens.FieldOfView);
    }

    public void ZoomIn()
    {
        cinemachineCamera.Lens.FieldOfView -= zoomStep;
        if (cinemachineCamera.Lens.FieldOfView < minFOV)
            cinemachineCamera.Lens.FieldOfView = minFOV;

        cameraFOVDebug?.UpdateFOVText((int)cinemachineCamera.Lens.FieldOfView);
    }

    public void ZoomOut()
    {
        cinemachineCamera.Lens.FieldOfView += zoomStep;
        if (cinemachineCamera.Lens.FieldOfView > maxFOV)
            cinemachineCamera.Lens.FieldOfView = maxFOV;

        cameraFOVDebug?.UpdateFOVText((int)cinemachineCamera.Lens.FieldOfView);
    }
}

