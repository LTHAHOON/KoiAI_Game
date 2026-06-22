using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _sfxSourcePrefab;
    [SerializeField]
    private PoolSize _sfxSourcePoolSize;

    private HashSet<AudioSFXTarget> _sfxTargetHashSet = new();
    private Pool<AudioSource> _sfxSourcePool;
    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EntityId entityId = _sfxSourcePrefab.GetEntityId();
        ulong id = EntityId.ToULong(entityId);
        PoolManager.Instance.AddPool(id, _sfxSourcePrefab, _sfxSourcePoolSize, PoolName.AudioSource);
        PoolManager.Instance.TryGetPool(id, out _sfxSourcePool);
    }

    public void UpdateSFXAudioPos(AudioSFXTarget audioTarget, Vector3 audioPos)
    {
        if (!_sfxTargetHashSet.Contains(audioTarget))
        {
            return;
        }
        AudioSource audioSource = audioTarget.GetAudioSource();
        audioSource.transform.position = audioPos;
    }

    public void PlayMusic(AudioMusicTarget audioTarget)
    {
        AudioSource audioSource = audioTarget.GetAudioSource();
        if(audioSource == null)
        {
            return;
        }
        audioSource.Play();
    }

    public void StopMusic(AudioMusicTarget audioTarget)
    {
        AudioSource audioSource = audioTarget.GetAudioSource();
        if (audioSource == null)
        {
            return;
        }
        audioSource.Stop();
    }

    public void PlaySFX(AudioSFXTarget audioTarget, AudioData audioData, Vector3 audioPos)
    {
        if (!_sfxTargetHashSet.Contains(audioTarget))
        {
            AudioSource newAudioSource = _sfxSourcePool.Pop();
            audioTarget.SetAudioSource(newAudioSource);
            _sfxTargetHashSet.Add(audioTarget);
        }
        
        AudioSource audioSource = audioTarget.GetAudioSource();
        if (audioSource == null || audioData == null || audioData.AudioResource == null)
        {
            return;
        }

        if (audioData.IsLoop)
        {
            audioSource.transform.position = audioPos;
            audioSource.resource = audioData.AudioResource;
            audioSource.loop = audioData.IsLoop;
            audioSource.volume = audioData.Volume;
            audioSource.Play();
        }
        else
        {
            if(audioData.AudioResource is AudioClip audioClip)
            {
                audioSource.PlayOneShot(audioClip, audioData.Volume);
            }
        }
    }

    public void FadeStopSFX(AudioSFXTarget audioTarget, float fadeTime)
    {
        AudioSource audioSource = audioTarget.GetAudioSource();
        if (audioSource == null)
        {
            return;
        }
        audioSource.loop = false;
        StartCoroutine(IEFadeStopSFX(audioSource, fadeTime));
    }

    private IEnumerator IEFadeStopSFX(AudioSource audioSource, float fadeTime)
    {
        float curTime = 0;
        float curVolume = audioSource.volume;
        while(curTime < fadeTime)
        {
            curTime += Time.deltaTime;
            float volume = curVolume - (fadeTime / curTime);
            audioSource.volume = volume;
            yield return null;
        }
        audioSource.Stop();
    }
    public void ReturnSFX(AudioSFXTarget audioTarget)
    {
        AudioSource audioSource = audioTarget.GetAudioSource();
        if (audioSource == null)
        {
            return;
        }
        audioSource.Stop();
        audioSource.transform.position = Vector3.zero;
        audioSource.clip = null;
        audioSource.loop = false;
        audioSource.volume = 0;
        
        _sfxSourcePool.Return(audioSource);
        _sfxTargetHashSet.Remove(audioTarget);
    }
}
