using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 27.5f, -12.5f);
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool noLerp = true;

    private void LateUpdate()
    {
        if (!target) return;

        if (noLerp)
        {
            transform.position = offset + target.position;
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}
