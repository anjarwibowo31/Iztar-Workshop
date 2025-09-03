using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 Direction { get => _direction; private set => _direction = value; }

    [Title("Joystick Settings")]
    public string joystickName = "Main Joystick";  // Nama joystick

    [Title("UI References")]
    public RectTransform background;   // Lingkaran luar
    public RectTransform handle;       // Lingkaran dalam (knob)
    public float handleLimit = 100f;    // Radius maksimal handle bergerak

    [ShowInInspector, ReadOnly]
    private Vector2 _direction;

    public float Horizontal => _direction.x;
    public float Vertical => _direction.y;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, handleLimit);
        handle.anchoredPosition = pos;
        _direction = pos / handleLimit;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _direction = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
