using UnityEngine;
using UnityEngine.Audio;

namespace KoiAI.Audio
{
    [CreateAssetMenu(fileName = "New AudioData", menuName = "Audio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [SerializeField]
        private AudioResource _audioResource;
        [SerializeField]
        private float _volume;
        [SerializeField]
        private bool _isLoop;
    
        public AudioResource AudioResource => _audioResource;
        public float Volume => _volume;
        public bool IsLoop => _isLoop;
    }
}
