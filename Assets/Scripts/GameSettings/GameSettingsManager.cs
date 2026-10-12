
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSettingsManager
{
    public static event Action Changed;

    private const string MusicKey = "musicVolume";
    private const string SfxKey = "soundsVolume";
    private const string MuteMusicKey = "settings.muteMusic";
    private const string MuteSfxKey = "settings.muteSfx";
    private const string MuteAllKey = "settings.muteAll";
    private const string FullscreenKey = "settings.fullscreen";
    private const string FpsLimitKey = "settings.fpsLimit";
    private const string ShowFpsKey = "settings.showFps";

    private static bool loaded;

    private static int musicVolume;
    private static int sfxVolume;
    private static bool muteMusic;
    private static bool muteSfx;
    private static bool muteAll;
    private static bool fullscreen;
    private static int fpsLimit;
    private static bool showFps;

    public static int MusicVolume
    {
        get { EnsureLoaded(); return musicVolume; }
    }

    public static int SfxVolume
    {
        get { EnsureLoaded(); return sfxVolume; }
    }

    public static bool MuteMusic
    {
        get { EnsureLoaded(); return muteMusic; }
    }

    public static bool MuteSfx
    {
        get { EnsureLoaded(); return muteSfx; }
    }

    public static bool MuteAll
    {
        get { EnsureLoaded(); return muteAll; }
    }

    public static bool Fullscreen
    {
        get { EnsureLoaded(); return fullscreen; }
    }

    public static int FpsLimit
    {
        get { EnsureLoaded(); return fpsLimit; }
    }

    public static bool ShowFps
    {
        get { EnsureLoaded(); return showFps; }
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeRuntime()
    {
        loaded = false;
        EnsureLoaded();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        ApplyAll();
    }

    private static void OnSceneLoaded(
        Scene scene, LoadSceneMode mode)
    {
        ApplyAll();
    }

    private static void EnsureLoaded()
    {
        if (loaded) return;

        musicVolume = Mathf.Clamp(
            PlayerPrefs.GetInt(MusicKey, 10), 0, 20);

        sfxVolume = Mathf.Clamp(
            PlayerPrefs.GetInt(SfxKey, 10), 0, 20);

        muteMusic = PlayerPrefs.GetInt(MuteMusicKey, 0) == 1;
        muteSfx = PlayerPrefs.GetInt(MuteSfxKey, 0) == 1;
        muteAll = PlayerPrefs.GetInt(MuteAllKey, 0) == 1;

        fullscreen = PlayerPrefs.GetInt(
            FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        fpsLimit = PlayerPrefs.GetInt(FpsLimitKey, 60);
        if (fpsLimit != 30 && fpsLimit != 60 && fpsLimit != -1)
            fpsLimit = 60;

        showFps = PlayerPrefs.GetInt(ShowFpsKey, 0) == 1;

        loaded = true;
    }

    private static void Save()
    {
        PlayerPrefs.SetInt(MusicKey, musicVolume);
        PlayerPrefs.SetInt(SfxKey, sfxVolume);
        PlayerPrefs.SetInt(MuteMusicKey, muteMusic ? 1 : 0);
        PlayerPrefs.SetInt(MuteSfxKey, muteSfx ? 1 : 0);
        PlayerPrefs.SetInt(MuteAllKey, muteAll ? 1 : 0);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(FpsLimitKey, fpsLimit);
        PlayerPrefs.SetInt(ShowFpsKey, showFps ? 1 : 0);

        PlayerPrefs.Save();
    }

    private static float ToDecibels(int volume, bool muted)
    {
        if (muted || volume <= 0) return -80f;
        return HelperUtilities.LinearToDecibels(volume);
    }

    public static void ApplyAudio()
    {
        EnsureLoaded();

        // Master mute: tat tat ca am thanh, khong sua volume cu.
        AudioListener.volume = muteAll ? 0f : 1f;

        GameResources resources = GameResources.Instance;
        if (resources == null) return;

        if (resources.musicMasterMixerGroup != null)
        {
            resources.musicMasterMixerGroup.audioMixer.SetFloat(
                "musicVolume",
                ToDecibels(musicVolume, muteMusic)
            );
        }

        if (resources.soundsMasterMixerGroup != null)
        {
            resources.soundsMasterMixerGroup.audioMixer.SetFloat(
                "soundsVolume",
                ToDecibels(sfxVolume, muteSfx)
            );
        }
    }

    public static void ApplyDisplay()
    {
        EnsureLoaded();

        if (!Application.isMobilePlatform)
        {
            Screen.fullScreenMode = fullscreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

            // De Application.targetFrameRate dieu khien FPS.
            QualitySettings.vSyncCount = 0;
        }

        // Mobile khong dung Unlimited.
        int actualLimit =
            Application.isMobilePlatform && fpsLimit == -1
                ? 60
                : fpsLimit;

        Application.targetFrameRate = actualLimit;
    }

    public static void ApplyAll()
    {
        ApplyAudio();
        ApplyDisplay();
    }

    private static void AudioChanged()
    {
        Save();
        ApplyAudio();
        Changed?.Invoke();
    }

    private static void DisplayChanged()
    {
        Save();
        ApplyDisplay();
        Changed?.Invoke();
    }

    public static void SetMusicVolume(int value)
    {
        EnsureLoaded();
        musicVolume = Mathf.Clamp(value, 0, 20);
        AudioChanged();
    }

    public static void SetSfxVolume(int value)
    {
        EnsureLoaded();
        sfxVolume = Mathf.Clamp(value, 0, 20);
        AudioChanged();
    }

    public static void ToggleMuteMusic()
    {
        EnsureLoaded();
        muteMusic = !muteMusic;
        AudioChanged();
    }

    public static void ToggleMuteSfx()
    {
        EnsureLoaded();
        muteSfx = !muteSfx;
        AudioChanged();
    }

    public static void ToggleMuteAll()
    {
        EnsureLoaded();
        muteAll = !muteAll;
        AudioChanged();
    }

    public static void ToggleFullscreen()
    {
        EnsureLoaded();
        if (Application.isMobilePlatform) return;

        fullscreen = !fullscreen;
        DisplayChanged();
    }

    public static void SetFpsLimit(int value)
    {
        EnsureLoaded();

        if (value != 30 && value != 60 && value != -1)
            return;

        if (Application.isMobilePlatform && value == -1)
            return;

        fpsLimit = value;
        DisplayChanged();
    }

    public static void CycleFpsLimit()
    {
        EnsureLoaded();

        if (Application.isMobilePlatform)
        {
            SetFpsLimit(fpsLimit == 30 ? 60 : 30);
        }
        else
        {
            int next = fpsLimit == 30 ? 60 :
                       fpsLimit == 60 ? -1 : 30;

            SetFpsLimit(next);
        }
    }

    public static void ToggleShowFps()
    {
        EnsureLoaded();
        showFps = !showFps;
        Save();
        Changed?.Invoke();
    }

    public static void ResetDefaults()
    {
        EnsureLoaded();

        musicVolume = 10;
        sfxVolume = 10;

        muteMusic = false;
        muteSfx = false;
        muteAll = false;

        fullscreen = true;
        fpsLimit = 60;
        showFps = false;

        Save();
        ApplyAll();
        Changed?.Invoke();
    }
}
