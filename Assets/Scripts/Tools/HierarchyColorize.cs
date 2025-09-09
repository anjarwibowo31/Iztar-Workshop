#if UNITY_EDITOR
using UnityEngine;

public class HierarchyColorize : MonoBehaviour
{
    [Header("Hierarchy Background")]
    public Color backgroundColor = Color.cyan;

    [Header("Text Settings")]
    public Color textColor = Color.black;
    public Color shadowColor = new Color(0f, 0f, 0f, 0f); // hitam transparan
    [Range(0f, 3f)] public float shadowOffset = 0; // ketebalan/geser shadow
}
#endif