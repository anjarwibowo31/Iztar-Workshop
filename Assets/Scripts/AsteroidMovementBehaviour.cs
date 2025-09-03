using UnityEngine;

public class AsteroidMovementBehaviour : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 25f; // derajat per detik
    private Vector3 moveDirection;
    private bool isMoving = false;
    private float fixedY;

    private Vector3 rotationAxis;

    private void Start()
    {
        fixedY = transform.position.y;
    }

    private void Update()
    {
        if (isMoving)
        {
            // Rotasi infinite di semua axis
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector3 newPos = transform.position + speed * Time.fixedDeltaTime * moveDirection;
            transform.position = new Vector3(newPos.x, fixedY, newPos.z);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Hitung arah menjauhi collider
        Vector3 direction = (transform.position - collider.transform.position).normalized;
        moveDirection = direction;

        // Tentukan axis rotasi berdasarkan arah tabrakan (biar unik tiap tabrakan)
        rotationAxis = direction.normalized;

        // Kalau arahnya nol (jarang terjadi), fallback ke acak
        if (rotationAxis == Vector3.zero)
        {
            rotationAxis = Random.onUnitSphere;
        }

        isMoving = true;
    }
}
