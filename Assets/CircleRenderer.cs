using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleRenderer : MonoBehaviour
{
    public int segments = 50;
    public float radius = 5f;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.loop = true;
        line.useWorldSpace = false; // penting supaya circle ikut parent

        DrawCircle();
    }

    void DrawCircle()
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            // Posisi circle dalam local space
            line.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}
