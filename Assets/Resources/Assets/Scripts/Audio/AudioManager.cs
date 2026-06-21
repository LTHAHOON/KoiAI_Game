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

    private HashSet<AudioTarget> _audioTargetHashSet = new();
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

    public void PlaySFX(AudioTarget audioTarget, AudioData audioData, Vector3 audioPos)
    {
        if (!_audioTargetHashSet.Contains(audioTarget))
        {
            AudioSource newAudioSource = _sfxSourcePool.Pop();
            audioTarget.SetAudioSource(newAudioSource);
            _audioTargetHashSet.Add(audioTarget);
        }
        
        AudioSource audioSource = audioTarget.GetAudioSource();
        if (audioSource == null || audioData == null || audioData.AudioClip == null)
        {
            return;
        }

        if (audioData.IsLoop)
        {
            audioSource.transform.position = audioPos;
            audioSource.clip = audioData.AudioClip;
            audioSource.loop = true;
            audioSource.volume = audioData.Volume;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(audioData.AudioClip, audioData.Volume);
        }
    }

    public void ReturnAudioSource(AudioTarget audioTarget)
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
        _audioTargetHashSet.Remove(audioTarget);
    }
}
