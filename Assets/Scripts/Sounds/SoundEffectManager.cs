using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonobehaviour<SoundEffectManager>
{
    public int soundsVolume = 10;

    public void PlaySoundEffect(SoundEffectSO soundEffect){
        SoundEffect sound = (SoundEffect) PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        sound.SetSound(soundEffect);
        sound.gameObject.SetActive(true);
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }

    private IEnumerator DisableSound(SoundEffect sound, float soundDuration){
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }
    
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
        soundsVolume = GameSettingsManager.SfxVolume;
    }

    public void IncreaseSoundVolume()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume + 1
        );
    }

    public void DecreaseSoundVolume()
    {
        GameSettingsManager.SetSfxVolume(
            GameSettingsManager.SfxVolume - 1
        );
    }

    private void SetSoundsVolume(int value)
    {
        GameSettingsManager.SetSfxVolume(value);
    }
}
