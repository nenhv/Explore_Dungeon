using TMPro;
using UnityEngine;

public class FPSCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;

    private float elapsedTime;
    private int frameCount;

    private void OnEnable()
    {
        GameSettingsManager.Changed += RefreshVisibility;
        RefreshVisibility();
    }

    private void OnDisable()
    {
        GameSettingsManager.Changed -= RefreshVisibility;
    }

    private void RefreshVisibility()
    {
        if (fpsText != null)
        {
            fpsText.enabled = GameSettingsManager.ShowFps;
        }
    }

    private void Update()
    {
        if (!GameSettingsManager.ShowFps) return;

        elapsedTime += Time.unscaledDeltaTime;
        frameCount++;

        if (elapsedTime >= 0.5f)
        {
            int fps = Mathf.RoundToInt(
                frameCount / elapsedTime);

            if (fpsText != null)
            {
                fpsText.text = "FPS: " + fps;
            }

            elapsedTime = 0f;
            frameCount = 0;
        }
    }
}
