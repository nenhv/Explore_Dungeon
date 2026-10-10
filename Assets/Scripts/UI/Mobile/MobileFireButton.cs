
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class MobileFireButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    // Nút FIRE đang hoạt động trong scene.
    public static MobileFireButton Instance { get; private set; }

    // Lưu pointer đang giữ nút.
    private int activePointerId = int.MinValue;

    // PlayerControl sẽ đọc giá trị này.
    public bool IsPressed =>
        activePointerId != int.MinValue;

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Only one MobileFireButton is allowed.",
                this
            );

            enabled = false;
            return;
        }

        Instance = this;
        ResetButton();
    }

    // Được gọi khi ngón tay bắt đầu chạm FIRE.
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        if (IsPressed)
            return;

        activePointerId = eventData.pointerId;
    }

    // Được gọi khi ngón tay nhấc lên.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        ResetButton();
    }

    private void ResetButton()
    {
        activePointerId = int.MinValue;
    }

    private void OnDisable()
    {
        ResetButton();

        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ResetButton();
    }
}
