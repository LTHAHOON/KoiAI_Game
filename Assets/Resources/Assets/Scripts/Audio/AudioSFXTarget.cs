using System;
using UnityEngine;

public class AudioSFXTarget : MonoBehaviour
{
    private AudioSource _audioSource;

    public void SetAudioSource(AudioSource audioSource)
    {
        _audioSource = audioSource;
    }
    
    public AudioSource GetAudioSource()
    {
        return _audioSource;
    }

    public bool IsPlayingSFX()
    {
        if(_audioSource == null)
        {
            return false;
        }
        return _audioSource.isPlaying;
    }

    private void OnDestroy()
    {
        AudioManager.Instance.ReturnSFX(this);
    }
}
