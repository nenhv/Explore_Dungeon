
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Cac loai hanh dong cua nut cam ung.
public enum MobileActionType
{
    Roll,
    Reload,
    Use,
    SwitchWeapon
}

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class MobileActionButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Action Settings")]
    [SerializeField]
    private MobileActionType actionType;

    // Moi loai hanh dong chi dang ky mot nut.
    private static readonly Dictionary<
        MobileActionType,
        MobileActionButton
    > buttons = new Dictionary<
        MobileActionType,
        MobileActionButton
    >();

    // Pointer dang giu nut.
    private int activePointerId = int.MinValue;

    // Mot lan cham tao ra mot yeu cau hanh dong.
    private bool pendingPress;

    // Reset du lieu static khi bat dau mot phien choi moi.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        buttons.Clear();
    }

    private void OnEnable()
    {
        MobileActionButton existingButton;

        if (buttons.TryGetValue(
            actionType,
            out existingButton))
        {
            if (existingButton != null &&
                existingButton != this)
            {
                Debug.LogWarning(
                    "Duplicate MobileActionButton: " +
                    actionType,
                    this
                );

                enabled = false;
                return;
            }
        }

        buttons[actionType] = this;
        ResetState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Chi nhan cham hoac chuot trai.
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        // Nut dang duoc giu boi pointer khac.
        if (activePointerId != int.MinValue)
            return;

        activePointerId = eventData.pointerId;

        // Danh dau co mot lan nhan moi.
        pendingPress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        // Nha nut khong tao them hanh dong.
        activePointerId = int.MinValue;
    }

    // PlayerControl goi de nhan MOT lan nhan.
    public static bool ConsumePress(MobileActionType type)
    {
        MobileActionButton button;

        if (!buttons.TryGetValue(type, out button))
            return false;

        if (button == null || !button.pendingPress)
            return false;

        // Da nhan yeu cau -> khong lap lai moi frame.
        button.pendingPress = false;

        return true;
    }

    // Xoa yeu cau chua xu ly.
    public static void ClearPendingPresses()
    {
        foreach (MobileActionButton button in buttons.Values)
        {
            if (button != null)
                button.pendingPress = false;
        }
    }

    // Huy tat ca trang thai khi Player bi khoa.
    public static void ResetAllInputs()
    {
        foreach (MobileActionButton button in buttons.Values)
        {
            if (button != null)
                button.ResetState();
        }
    }

    private void ResetState()
    {
        activePointerId = int.MinValue;
        pendingPress = false;
    }

    private void OnDisable()
    {
        MobileActionButton registeredButton;

        if (buttons.TryGetValue(
                actionType,
                out registeredButton) &&
            registeredButton == this)
        {
            buttons.Remove(actionType);
        }

        ResetState();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ResetState();
    }
}
