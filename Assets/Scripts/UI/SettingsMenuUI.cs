
using UnityEngine;
using TMPro;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private TMP_Text muteMusicButtonText;
    [SerializeField] private TMP_Text muteSfxButtonText;
    [SerializeField] private TMP_Text muteAllButtonText;

    [Header("Display")]
    [SerializeField] private TMP_Text fullscreenButtonText;
    [SerializeField] private TMP_Text fpsLimitButtonText;
    [SerializeField] private TMP_Text showFpsButtonText;
    [SerializeField] private GameObject fullscreenRow;

    private void OnEnable()
    {
        GameSettingsManager.Changed += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        GameSettingsManager.Changed -= RefreshUI;
    }

    private void RefreshUI()
    {
        musicValueText.text =
            GameSettingsManager.MusicVolume.ToString();

        sfxValueText.text =
            GameSettingsManager.SfxVolume.ToString();

        muteMusicButtonText.text =
            GameSettingsManager.MuteMusic
                ? "UNMUTE MUSIC"
                : "MUTE MUSIC";

        muteSfxButtonText.text =
            GameSettingsManager.MuteSfx
                ? "UNMUTE SFX"
                : "MUTE SFX";

        muteAllButtonText.text =
            GameSettingsManager.MuteAll
                ? "UNMUTE ALL"
                : "MUTE ALL";

        fullscreenButtonText.text =
            GameSettingsManager.Fullscreen
                ? "FULLSCREEN: ON"
                : "FULLSCREEN: OFF";

        int fpsLimit = GameSettingsManager.FpsLimit;

        fpsLimitButtonText.text = fpsLimit == -1
            ? "FPS: UNLIMITED"
            : "FPS: " + fpsLimit;

        showFpsButtonText.text =
            GameSettingsManager.ShowFps
                ? "SHOW FPS: ON"
                : "SHOW FPS: OFF";

        // Fullscreen chi danh cho PC.
        if (fullscreenRow != null)
        {
            fullscreenRow.SetActive(
                !Application.isMobilePlatform);
        }
    }

    // MUSIC
    public void IncreaseMusic()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume + 1);
    }

    public void DecreaseMusic()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume - 1);
    }

    // SOUND EFFECTS
    public void IncreaseSfx()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume + 1);
    }

    public void DecreaseSfx()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume - 1);
    }

    // MUTE
    public void ToggleMuteMusic()
    {
        GameSettingsManager.ToggleMuteMusic();
    }

    public void ToggleMuteSfx()
    {
        GameSettingsManager.ToggleMuteSfx();
    }

    public void ToggleMuteAll()
    {
        GameSettingsManager.ToggleMuteAll();
    }

    // DISPLAY
    public void ToggleFullscreen()
    {
        GameSettingsManager.ToggleFullscreen();
    }

    public void CycleFpsLimit()
    {
        GameSettingsManager.CycleFpsLimit();
    }

    public void ToggleShowFps()
    {
        GameSettingsManager.ToggleShowFps();
    }

    // SYSTEM
    public void ResetDefaults()
    {
        GameSettingsManager.ResetDefaults();
    }

    public void BackToMenu()
    {
        gameObject.SetActive(false);
    }
}
