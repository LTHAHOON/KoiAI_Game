using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace KoiAI.Player
{
    using KoiAI.Audio;
    using KoiAI.Skin;
    
    public abstract class PlayerFeatureExtensionData { }
    
    [Serializable]
    public abstract class PlayerFeatureValueData { }

    public abstract class PlayerFeature : MonoBehaviour 
    {
        //아이템에서 접근을 가능하게 해주는 Key역할을 합니다.
        public enum PlayerFeatureProperty
        {
            None,
            Movement,
            Rotation,
            Equipment,
            WayPoint,
        }
        
        public abstract PlayerFeatureProperty FeatureProperty { get; }
        public PlayerController Owner { get; set; }
        public virtual void Init(PlayerInputAction playerIA, PlayerFeatureValueData playerFeatureValueData = null, 
                                    PlayerFeatureExtensionData playerFeatureExtensionData = null) { }
        public abstract void UpdateFeature();
    }

    [Serializable]
    public struct PlayerSFXAudioFeature
    {
        public enum PlayerSFXAuidoProperty
        {
            Main,
            Move,
            Attack,
        }
        [SerializeField]
        private PlayerSFXAuidoProperty _playerAudioProperty;
        [SerializeField]
        private AudioSFXTarget _auidoTarget;

        public PlayerSFXAuidoProperty PlayerAudioProperty => _playerAudioProperty;
        public AudioSFXTarget AudioTarget => _auidoTarget;
    }

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] 
        private PlayerData _playerData;
        [SerializeField] 
        private PlayableDirector _timeline;
        [SerializeField]
        private PlayerFeature[] _playerFeatures;
        [SerializeField]
        private PlayerSFXAudioFeature[] _sfxFeatures;
        [SerializeField]
        private LayerMask _targetLayerMask;
        [Header("현재 스킨")]
        [ReadOnly]
        [SerializeField]
        private PlayerSkin _curPlayerSkin;
    
        private readonly Dictionary<int, PlayerFeature> _dicPlayerFeatures = new();
        private PlayerInput _playerInput;
        private PlayerInputAction _playerInputAction;
        private void Awake()
        {
            if (!_playerData)
            {
                Debug.LogError("PlayerData is null");
                return;
            }
            //PlayerInput 바인딩
            _playerInput = GetComponent<PlayerInput>();
            _playerFeatures = GetComponents<PlayerFeature>();
            _playerInputAction = new();
            _playerInput.actions = _playerInputAction.asset;
            PlayerSkin playerSkin = _playerData.PlayerSkin;
            GameObject newPlayerSkinObj = Instantiate(playerSkin.SkinPrefab, transform);
            _curPlayerSkin = newPlayerSkinObj.GetComponent<PlayerSkin>();
        }
        private void Start()
        {
            for (int i = 0; i < _playerFeatures.Length; i++)
            {
                PlayerFeature.PlayerFeatureProperty featureProperty = _playerFeatures[i].FeatureProperty;
                if (featureProperty != PlayerFeature.PlayerFeatureProperty.None)
                {
                    _dicPlayerFeatures.Add((int)featureProperty, _playerFeatures[i]);
                }
                _playerFeatures[i].Owner = this;
                PlayerFeatureValueData valueData = _playerData.GetPlayerFeatureValueData(featureProperty);
                PlayerFeatureExtensionData extensionData = _playerData.GetPlayerFeatureExtensionData(featureProperty);
                _playerFeatures[i].Init(_playerInputAction, valueData, extensionData);
            }
        }

        private void Update()
        {
            if (_timeline.state == PlayState.Playing)
            {
                return;
            }
            for (int i = 0; i < _playerFeatures.Length; i++)
            {
                _playerFeatures[i].UpdateFeature();
            }
        }

        public PlayerFeature GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty featureProperty)
        {
            bool bGet = _dicPlayerFeatures.TryGetValue((int)featureProperty, out PlayerFeature playerFeature);
            if(!bGet)
            {
                Debug.LogError($"Error: _dicPlayerFeatures has not {featureProperty}");
            }
            return playerFeature;
        }

        public AudioSFXTarget GetAudioSFXTarget(PlayerSFXAudioFeature.PlayerSFXAuidoProperty sfxProperty)
        {
            //Player AudioSFXTarget은 갯수가 적은 편이기 때문에 딕셔너리 대신 For문 사용
            for (int i = 0; i < _sfxFeatures.Length; i++)
            {
                if (_sfxFeatures[i].PlayerAudioProperty == sfxProperty)
                {
                    return _sfxFeatures[i].AudioTarget;
                }
            }
            return null;
        }

        public PlayerData PlayerData => _playerData;
        public PlayerInputAction PlayerIA=> _playerInputAction;
        public PlayerSkin CurrentPlayerSkin => _curPlayerSkin;
        public LayerMask TargetLayerMask => _targetLayerMask;
    }
}