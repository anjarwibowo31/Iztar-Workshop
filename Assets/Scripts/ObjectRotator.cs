using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class ObjectRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Title("Rotation Settings")]
    public Axis rotateAxis = Axis.Y;
    public float speed = 50f;

    private Action rotationMethod;

    void Start()
    {
        SetRotationMethod(rotateAxis);
    }

    void Update()
    {
        rotationMethod?.Invoke();
    }

    private void RotateX() => transform.Rotate(speed * Time.deltaTime * Vector3.right);
    private void RotateY() => transform.Rotate(speed * Time.deltaTime * Vector3.up);
    private void RotateZ() => transform.Rotate(speed * Time.deltaTime * Vector3.forward);

    private void SetRotationMethod(Axis axis)
    {
        switch (axis)
        {
            case Axis.X: rotationMethod = RotateX; break;
            case Axis.Y: rotationMethod = RotateY; break;
            case Axis.Z: rotationMethod = RotateZ; break;
        }
    }

    [Button("Set New Axis")]
    public void InvokeNewAxis()
    {
        SetRotationMethod(rotateAxis);
        Debug.Log($"Rotation axis changed to: {rotateAxis}");
    }
}
