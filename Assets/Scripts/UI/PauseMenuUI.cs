
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text musicLevelText;
    [SerializeField] private TMP_Text soundsLevelText;

    // Optional: gan neu them nut Mute All vao Pause Menu.
    [SerializeField] private TMP_Text muteAllButtonText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        RefreshUI();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void RefreshUI()
    {
        if (musicLevelText != null)
        {
            musicLevelText.text =
                GameSettingsManager.MusicVolume.ToString();
        }

        if (soundsLevelText != null)
        {
            soundsLevelText.text =
                GameSettingsManager.SfxVolume.ToString();
        }

        if (muteAllButtonText != null)
        {
            muteAllButtonText.text =
                GameSettingsManager.MuteAll
                    ? "UNMUTE ALL"
                    : "MUTE ALL";
        }
    }

    public void IncreaseMusicVolume()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume + 1);
        RefreshUI();
    }

    public void DecreaseMusicVolume()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume - 1);
        RefreshUI();
    }

    // Giu nguyen ten ham cu:
    // IncreaseSoundvolume (chu v viet thuong).
    public void IncreaseSoundvolume()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume + 1);
        RefreshUI();
    }

    public void DecreaseSoundsVolume()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume - 1);
        RefreshUI();
    }

    public void ToggleMuteAll()
    {
        GameSettingsManager.ToggleMuteAll();
        RefreshUI();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
