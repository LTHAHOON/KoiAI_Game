using System;
using UnityEngine;

public class AudioTarget : MonoBehaviour
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

    private void OnDestroy()
    {
        AudioManager.Instance.ReturnAudioSource(this);
    }
}
