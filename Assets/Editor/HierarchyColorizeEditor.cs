#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyColorizeEditor
{
    static HierarchyColorizeEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        var colorize = obj.GetComponent<HierarchyColorize>();
        if (colorize == null) return;

        // --- Draw background ---
        EditorGUI.DrawRect(selectionRect, colorize.backgroundColor);

        // --- Styles ---
        GUIStyle shadowStyle = new GUIStyle(EditorStyles.label);
        shadowStyle.normal.textColor = colorize.shadowColor;

        GUIStyle textStyle = new GUIStyle(EditorStyles.label);
        textStyle.normal.textColor = colorize.textColor;

        // --- Shadow rect (geser sesuai offset) ---
        Rect shadowRect = new Rect(
            selectionRect.x + colorize.shadowOffset,
            selectionRect.y + colorize.shadowOffset,
            selectionRect.width,
            selectionRect.height
        );

        // --- Draw shadow ---
        EditorGUI.LabelField(shadowRect, obj.name, shadowStyle);

        // --- Draw text utama ---
        EditorGUI.LabelField(selectionRect, obj.name, textStyle);
    }
}
#endif
