using System;
using KoiAI.Audio;
using R3;
using UnityEngine;

namespace KoiAI.Interact
{
    [Serializable]
    public enum CrateKeyType
    {
        None,
        CrateKey_01
    }

    public enum CrateInteractType
    {
        Close,
        Open
    }

    public struct CrateInteractContext
    {
        public CrateInteractType InteractType {get; set;} 
        public CrateKeyType KeyType {get; set;}
    }

    public class CrateInteractable : BaseInteractable<CrateInteractContext>
    {
        [SerializeField]
        private bool _onlyOpen = true;
        [SerializeField]
        private string _isOpenedParam;
        [SerializeField]
        private Animator _crateAnimator;
        [SerializeField]
        private CrateKeyType _keyType;
        [SerializeField]
        private AudioSFXTarget _audioSFXTarget;
        [SerializeField]
        private AudioData _openAudioData;
        [SerializeField]
        private AudioData _closeAudioData;
        [SerializeField]
        private float _throttleFirstTime = 2f;

        private bool _isOpened = false;
        private int _isOpenedParamHash;
        
        private IDisposable _interactDisposable;
        private void Awake()
        {
            _isOpenedParamHash = Animator.StringToHash(_isOpenedParam);

            _interactDisposable = OnInteract
            .Where(context => context.KeyType == _keyType && (_onlyOpen ? !_isOpened : true))
            .ThrottleFirst(TimeSpan.FromSeconds(_throttleFirstTime))
            .Subscribe(_ => OpenOrClose())
            .AddTo(this);
        }

        private void OpenOrClose()
        {
            _crateAnimator.SetBool(_isOpenedParamHash, !_isOpened);
            AudioData audioData = _isOpened ? _openAudioData : _closeAudioData;
            AudioManager.Instance.PlaySFX(_audioSFXTarget, audioData, transform.position);

            if(_onlyOpen)
            {
                _interactDisposable.Dispose();
            }
        }
    }
}
