using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Iztar.InputSystem
{
    public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [ShowInInspector, ReadOnly]
        private Vector2 _direction;

        [Title("Joystick Settings")]
        public string joystickName = "Main Joystick";
        [SerializeField] private float handleLimit = 100f;
        [SerializeField] private float directionMaxLength = 1.5f;
        [SerializeField] private float inputDeadzone = 0.01f;
        [SerializeField] private float inputSensitivity = 1f;

        [Title("UI References")]
        [SerializeField] private RectTransform joystickVisual;
        [SerializeField] private RectTransform touchHandle;
        [SerializeField] private RectTransform idleVisual;
        [SerializeField] private RectTransform directionVisual;

        private RectTransform joystickArea;
        private Vector2 joystickDefaultPos;

        // Events: backward compatible and new granular events
        public event Action<Vector2> OnDirectionChanged;
        public event Action<Vector2> OnDirectionPerformed;
        public event Action OnDirectionCancelled;

        public Vector2 Direction
        {
            get => _direction;
            private set
            {
                if (_direction == value) return;

                _direction = value;

                // Always notify change (including zero)
                OnDirectionChanged?.Invoke(_direction);

                // Granular events with deadzone
                float dz2 = inputDeadzone * inputDeadzone;
                if (_direction.sqrMagnitude > dz2)
                    OnDirectionPerformed?.Invoke(_direction);
                else
                    OnDirectionCancelled?.Invoke();
            }
        }

        public float Horizontal => _direction.x;
        public float Vertical => _direction.y;

        private void Awake()
        {
            joystickArea = GetComponent<RectTransform>();

            joystickVisual.anchorMin = joystickArea.anchorMin;
            joystickVisual.anchorMax = joystickArea.anchorMin;
            joystickVisual.pivot = new Vector2(0.5f, 0.5f);

            SetActiveState(false);
            joystickDefaultPos = joystickVisual.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickVisual.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

            joystickVisual.anchoredPosition = localPoint;

            OnDrag(eventData);
            SetActiveState(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickVisual,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 pos);

            pos = Vector2.ClampMagnitude(pos, handleLimit);
            touchHandle.anchoredPosition = pos;

            Direction = pos / handleLimit;

            UpdateDirectionVisual();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            touchHandle.anchoredPosition = Vector2.zero;

            UpdateDirectionVisual();
            SetActiveState(false);
            joystickVisual.anchoredPosition = joystickDefaultPos;
        }

        private void UpdateDirectionVisual()
        {
            if (directionVisual == null) return;

            if (_direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                directionVisual.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
            else
            {
                directionVisual.localRotation = Quaternion.identity;
            }
        }

        private void SetActiveState(bool isActiveTouching)
        {
            if (idleVisual != null) idleVisual.gameObject.SetActive(!isActiveTouching);
            if (touchHandle != null) touchHandle.gameObject.SetActive(isActiveTouching);
            if (directionVisual != null) directionVisual.gameObject.SetActive(isActiveTouching);
        }
    }
}