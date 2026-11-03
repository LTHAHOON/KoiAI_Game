using System.Collections;
using System.Collections.Generic;
using KoiAI.Pool;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.Audio
{
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
            ulong id =  _sfxSourcePrefab.GetEntityULongID();
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

            if (fadeTime <= 0f)
            {
                audioSource.Stop();
                audioSource.volume = 0f;
                audioSource.loop = false;
                return;
            }

            StartCoroutine(IEFadeStopSFX(audioSource, fadeTime));
        }

        private IEnumerator IEFadeStopSFX(AudioSource audioSource, float fadeTime)
        {
            float curTime = 0;
            float curVolume = audioSource.volume;
            while(curTime < fadeTime)
            {
                curTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(curTime / fadeTime);
                audioSource.volume = Mathf.Lerp(curVolume, 0f, normalizedTime);
                yield return null;
            }
            audioSource.volume = 0f;
            audioSource.loop = false;
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
}
