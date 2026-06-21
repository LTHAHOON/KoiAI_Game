using UnityEngine;

[CreateAssetMenu(fileName = "New AudioData", menuName = "Audio/AudioData")]
public class AudioData : ScriptableObject
{
    [SerializeField]
    private AudioClip _audioClip;
    [SerializeField]
    private float _volume;
    [SerializeField]
    private bool _isLoop;
    
    public AudioClip AudioClip => _audioClip;
    public float Volume => _volume;
    public bool IsLoop => _isLoop;
}
