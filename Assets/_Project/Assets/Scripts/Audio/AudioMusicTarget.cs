using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class AudioMusicTarget : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource> ();
    }

    public AudioSource GetAudioSource()
    {
        return _audioSource;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(GameTags.Player))
        {
            if(_audioSource == null)
            {
                return;
            }
            AudioManager.Instance.PlayMusic(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(GameTags.Player))
        {
            if (_audioSource == null)
            {
                return;
            }
            _audioSource.Stop();
        }
    }

}
