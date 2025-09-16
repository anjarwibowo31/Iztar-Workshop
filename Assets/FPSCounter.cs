using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private int frameCount = 0;
    private float elapsedTime = 0f;

    void Update()
    {
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= updateInterval)
        {
            float fps = frameCount / elapsedTime;
            fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";

            // reset
            frameCount = 0;
            elapsedTime = 0f;
        }
    }
}
