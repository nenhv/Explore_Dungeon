using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : SingletonMonobehaviour<MusicManager>
{
    private AudioSource musicAudioSource = null;
    private AudioClip currentAudioClip = null;
    private Coroutine fadeOutMusicCoroutine;
    private Coroutine fadeInMusicCoroutine;
    public int musicVolume = 10;

    private void OnEnable()
    {
        GameSettingsManager.Changed += SyncVolume;
    }

    private void Start()
    {
        SyncVolume();
        GameSettingsManager.ApplyAudio();
    }

    private void OnDisable()
    {
        GameSettingsManager.Changed -= SyncVolume;
    }

    private void SyncVolume()
    {
        musicVolume = GameSettingsManager.MusicVolume;
    }

    public void IncreaseMusicVolume()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume + 1
        );
    }

    public void DecreaseMusicVolume()
    {
        GameSettingsManager.SetMusicVolume(
            GameSettingsManager.MusicVolume - 1
        );
    }

    public void SetMusicVolume(int value)
    {
        GameSettingsManager.SetMusicVolume(value);
    }

    protected override void Awake(){
        base.Awake();

        musicAudioSource = GetComponent<AudioSource>();

        GameResources.Instance.musicOffSnapshot.TransitionTo(0f);
    }
    public void PlayMusic(MusicTrackSO musicTrack, float fadeOutTime = Settings.musicFadeOutTime, float fadeInTime = Settings.musicFadeInTime){
        StartCoroutine(PlayMusicRoutine(musicTrack, fadeOutTime, fadeInTime));
    }

    private IEnumerator PlayMusicRoutine(MusicTrackSO musicTrack, float fadeOutTime, float fadeInTime){
        if (fadeOutMusicCoroutine != null){
            StopCoroutine(fadeOutMusicCoroutine);
        }
        if (fadeInMusicCoroutine != null){
            StopCoroutine(fadeInMusicCoroutine);
        }
        if (musicTrack.musicClip != currentAudioClip){
            currentAudioClip = musicTrack.musicClip;
            yield return fadeOutMusicCoroutine = StartCoroutine(FadeOutMusic(fadeOutTime));

            yield return fadeInMusicCoroutine = StartCoroutine(FadeInMusic(musicTrack, fadeInTime));
        }

        yield return null;
    }

    private IEnumerator FadeOutMusic(float fadeOutTime){
        GameResources.Instance.musicLowSnapshot.TransitionTo(fadeOutTime);

        yield return new WaitForSeconds(fadeOutTime);
    }

    private IEnumerator FadeInMusic(MusicTrackSO musicTrack, float fadeInTime){
        musicAudioSource.clip = musicTrack.musicClip;
        musicAudioSource.volume = musicTrack.musicVolume;
        musicAudioSource.Play();

        GameResources.Instance.musicOnFullSnapshot.TransitionTo(fadeInTime);

        yield return new WaitForSeconds(fadeInTime);
    }
}
