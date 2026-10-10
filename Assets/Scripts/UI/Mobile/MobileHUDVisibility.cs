
using UnityEngine;

public class MobileHUDVisibility : MonoBehaviour
{
    [Header("Mobile HUD Settings")]
    [SerializeField]
    private bool showInEditor = true;

    private void Awake()
    {
        bool shouldShow = Application.platform == RuntimePlatform.Android;

#if UNITY_EDITOR
        shouldShow = showInEditor;
#endif

        gameObject.SetActive(shouldShow);
    }
}
