using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
public class ManualPreferredSizeFitter : MonoBehaviour
{
    [Tooltip("RectTransform yang akan diubah ukurannya. Jika kosong, pakai RectTransform ini.")]
    [SerializeField] private RectTransform target;

    [Header("Options")]
    private Vector2 minSize = Vector2.zero;
    private Vector2 maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
    [SerializeField] private bool includeInactiveChildren = false;

    /// <summary>
    /// Hitung ulang ukuran (width & height) berdasarkan PreferredSize layout anak-anak.
    /// Panggil method ini secara manual dari script kamu setelah ada perubahan.
    /// </summary>
    public void Recalculate()
    {
        if (target == null) target = GetComponent<RectTransform>();
        if (target == null) return;

        // Pastikan layout up-to-date
        LayoutRebuilder.ForceRebuildLayoutImmediate(target);

        float width  = LayoutUtility.GetPreferredSize(target, 0);
        float height = LayoutUtility.GetPreferredSize(target, 1);

        // Clamp
        width  = Mathf.Clamp(width,  minSize.x, maxSize.x);
        height = Mathf.Clamp(height, minSize.y, maxSize.y);

        // Apply
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    [Button]
    private void ContextRecalculate()
    {
        Recalculate();
    }
}
