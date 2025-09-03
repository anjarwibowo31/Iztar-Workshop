using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public struct CameraShakeParams
{
    public float amplitude;
    public float frequency;
    public float duration;
}

public class CameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Default Shake Parameters")]
    [SerializeField] private CameraShakeParams defaultShakeParams = new CameraShakeParams { amplitude = 2f, frequency = 2f, duration = 0.3f };

    private void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start()
    {
        ShipController.Instance.OnCollision += Instance_OnTriggerEnterEvent;
    }

    private void Instance_OnTriggerEnterEvent(float shipSpeed)
    {
        Shake(defaultShakeParams);
    }

    public void Shake(CameraShakeParams shakeParams)
    {
        if (noise == null) return;
        StartCoroutine(DoShake(shakeParams));
    }

    private IEnumerator DoShake(CameraShakeParams shakeParams)
    {
        // aktifkan shake
        noise.AmplitudeGain = shakeParams.amplitude;
        noise.FrequencyGain = shakeParams.frequency;

        yield return new WaitForSeconds(shakeParams.duration);

        // reset shake
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
}
