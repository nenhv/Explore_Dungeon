using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class MobileJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Joystick Settings")]
    [SerializeField, Range(0f, 0.5f)]
    private float deadZone = 0.1f;

    // Cho PlayerControl truy cập joystick đang hoạt động.
    public static MobileJoystick Instance { get; private set; }

    // Giá trị di chuyển: X và Y nằm trong khoảng -1 đến 1.
    public Vector2 Direction { get; private set; }

    // int.MinValue có nghĩa là chưa có ngón tay điều khiển.
    private int activePointerId = int.MinValue;

    public bool IsPressed => activePointerId != int.MinValue;

    
    private void OnEnable()
    {
        if (background == null || handle == null)
        {
            Debug.LogError(
                "MobileJoystick: Chua gan Background hoac Handle!",
                this
            );

            enabled = false;
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "MobileJoystick: Da ton tai joystick khac!",
                this
            );

            enabled = false;
            return;
        }

        Instance = this;
        ResetJoystick();
    }

    // Khi bắt đầu chạm vào joystick.
    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPressed)
            return;

        activePointerId = eventData.pointerId;

        UpdateJoystick(eventData);
    }

    // Khi kéo ngón tay trên màn hình.
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        UpdateJoystick(eventData);
    }

    // Khi nhấc ngón tay khỏi màn hình.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        ResetJoystick();
    }

    private void UpdateJoystick(PointerEventData eventData)
    {
        Vector2 localPoint;

        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

        if (!success)
            return;

        // Bán kính vùng joystick.
        float radius = Mathf.Min(
            background.rect.width,
            background.rect.height
        ) * 0.5f;

        if (radius <= 0f)
            return;

        // Chuyển vị trí ngón tay thành hướng từ -1 đến 1.
        Vector2 input = localPoint / radius;

        // Không cho độ lớn vector vượt quá 1.
        Direction = Vector2.ClampMagnitude(input, 1f);

        // Vùng chết: ngón tay gần tâm thì đứng yên.
        if (Direction.magnitude < deadZone)
        {
            Direction = Vector2.zero;
        }

        // Giữ nút nhỏ nằm trong nền joystick.
        float handleRadius = Mathf.Max(
            handle.rect.width,
            handle.rect.height
        ) * 0.5f;

        float maxDistance = Mathf.Max(
            0f,
            radius - handleRadius
        );

        handle.anchoredPosition = Direction * maxDistance;
    }

    private void ResetJoystick()
    {
        activePointerId = int.MinValue;
        Direction = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void OnDisable()
    {
        ResetJoystick();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
