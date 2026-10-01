using System;
using System.Collections.Generic;
using KoiAI.Audio;
using KoiAI.Item;
using KoiAI.Player;
using KoiAI.UI;
using UnityEngine;

namespace KoiAI.Interact
{
    [Serializable]
    public enum CrateKeyType
    {
        None,
        CrateKey_01
    }


    public struct CrateInteractContext
    {
        public CrateKeyType KeyType { get; set; }
    }

    public class CrateInteractable : BaseInteractable<CrateInteractContext>, IPlayerInteractable
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
        private List<ItemData> _dropItems = new();
        [SerializeField]
        private AudioSFXTarget _audioSFXTarget;
        [SerializeField]
        private AudioData _openAudioData;
        [SerializeField]
        private AudioData _closeAudioData;
        [SerializeField]
        private float _throttleFirstTime = 2f;
        [SerializeField]
        private float _showDistanceToPlayer = 5f;

        private bool _isOpened = false;
        private int _isOpenedParamHash;

        private bool _hasMatchingKey;
        private float _nextInteractTime;
        private bool _isOpenAnimationFinished = false;
        private bool _isVisible = false;
        private DropTableSelector _dropTableSelector;
        //싱글플레이임으로 리스트가 아닌 단일로 캐싱합니다.
        private PlayerInteractor _playerInteractor;

        public void Interact(PlayerInteractor playerInteractor)
        {
            //NextInteractTime수정하자
            _playerInteractor = playerInteractor;
            if ((_keyType != CrateKeyType.None && !_hasMatchingKey) ||
                (_onlyOpen && _isOpened) || Time.time < _nextInteractTime)
            {
                //  return;
            }
            if(_onlyOpen && _isOpened || (Time.time < _nextInteractTime))
            {
                return;
            }

            OpenOrClose();
            _nextInteractTime = Time.time + _throttleFirstTime;
        }

        public override void Interact(CrateInteractContext context)
        {
            if (_keyType == CrateKeyType.None || context.KeyType != _keyType)
            {
                return;
            }

            _hasMatchingKey = true;
            base.Interact(context);
        }

        private void Awake()
        {
            _isOpenedParamHash = Animator.StringToHash(_isOpenedParam);
            _dropTableSelector = FindAnyObjectByType<DropTableSelector>(FindObjectsInactive.Include);
        }
        
        
        private void Update()
        {
            if (!_isOpenAnimationFinished)
            {
                return;
            }
            float distanceToPlayer = Vector3.Distance(transform.position, _playerInteractor.transform.position);
            if (distanceToPlayer <= _showDistanceToPlayer)
            {
                if(!_isVisible)
                {
                    _dropTableSelector.ShowItems(_dropItems);
                    _isVisible = true;
                }
            }
            else
            {
                if(_isVisible)
                {
                    _dropTableSelector.HideItems();
                    _isVisible = false;
                }
            }
        }

        private void OnDisable()
        {
            if (_isOpened && _dropTableSelector != null)
            {
                _dropTableSelector.HideItems();
                _isVisible = false;
            }
        }

        private void OpenOrClose()
        {
            _isOpened = !_isOpened;
            _crateAnimator.SetBool(_isOpenedParamHash, _isOpened);
            AudioData audioData = _isOpened ? _openAudioData : _closeAudioData;
            AudioManager.Instance.PlaySFX(_audioSFXTarget, audioData, transform.position);
            if (_dropTableSelector == null)
            {
                return;
            }
        }

        public void OnOpenAnimationFinished()
        {
            _isOpenAnimationFinished = true;
        }

        public void OnCloseAnimationFinished()
        {
            _isOpenAnimationFinished = false;
        }
    }
}
