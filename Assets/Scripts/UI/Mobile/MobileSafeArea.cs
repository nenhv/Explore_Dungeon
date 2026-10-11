using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class MobileSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;

    private Rect lastSafeArea;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        Rect currentSafeArea = Screen.safeArea;

        // Chi cap nhat neu kich thuoc man hinh
        // hoac Safe Area thay doi.
        if (currentSafeArea != lastSafeArea ||
            Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null)
            return;

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;

        // Chuyen toa do pixel sang Anchor 0..1.
        Vector2 anchorMin = new Vector2(
            safeArea.xMin / Screen.width,
            safeArea.yMin / Screen.height
        );

        Vector2 anchorMax = new Vector2(
            safeArea.xMax / Screen.width,
            safeArea.yMax / Screen.height
        );

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Loai bo phan offset thua.
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Ghi nho trang thai da ap dung.
        lastSafeArea = safeArea;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
