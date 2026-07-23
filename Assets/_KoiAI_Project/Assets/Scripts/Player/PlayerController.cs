using NaughtyAttributes;
using System;
using System.Collections.Generic;
using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.Playables;
using static KoiAI.Player.PlayerFeature;
using static KoiAI.Player.PlayerSFXAudioFeature;

namespace KoiAI.Player
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Audio;
    using KoiAI.Camera;

    public abstract class PlayerFeatureExtensionData { }
    
    [Serializable]
    public abstract class PlayerFeatureValueData { }

    [RequireComponent(typeof(PlayerController))]
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
        
        public virtual void InitAutoInEnditor() { }
        
        public virtual void Init(PlayerFeatureValueData playerFeatureValueData = null, 
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

    [RequireComponent(typeof(PlayerAutoFeatureGenerator), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Cinemachine Data Mediator")]
        [SerializeField]
        private CinemachineDataMediator _playerCmDataMediator;
        [Header("플레이어 데이터")]
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
    
        private PlayerAutoFeatureGenerator _playerAutoFeatureGenerator;
        private readonly Dictionary<int, PlayerFeature> _dicPlayerFeatures = new();
        private Animator _playerAnimator;

        private void Awake()
        {
            _playerAutoFeatureGenerator = GetComponent<PlayerAutoFeatureGenerator>();

            Init();
        }

        private void Start()
        {
            for (int i = 0; i < _playerFeatures.Length; i++)
            {
                PlayerFeatureProperty featureProperty = _playerFeatures[i].FeatureProperty;
                if (featureProperty != PlayerFeatureProperty.None)
                {
                    _dicPlayerFeatures.Add((int)featureProperty, _playerFeatures[i]);
                }
                _playerFeatures[i].Owner = this;
                PlayerFeatureValueData valueData = _playerData.GetPlayerFeatureValueData(featureProperty);
                PlayerFeatureExtensionData extensionData = _playerData.GetPlayerFeatureExtensionData(featureProperty);
                _playerFeatures[i].Init(valueData, extensionData);
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

        private void Init()
        {
            _playerFeatures = GetComponents<PlayerFeature>();

            //스킨 생성
            _curPlayerSkin = CharacterSettingService.InstantiateCharacter(transform);
            _playerData = CharacterSettingService.GetCurrentPlayerData();
            if (!_playerData)
            {
                Debug.LogError("PlayerData is null");
                return;
            }
            _playerAutoFeatureGenerator.GeneratePlayerFeature();
            _playerCmDataMediator.ConnectHandlesInConnector();
            
            //Animator 초기화
            _playerAnimator = GetComponent<Animator>();
            AnimatorData animatorData = _playerData.AnimatorData;
            if (animatorData.IsValid())
            {
                _playerAnimator.runtimeAnimatorController = animatorData.RuntimeAnimController;
                _playerAnimator.avatar = animatorData.AnimatorAvatar;
            }
        }

        public void InitInEditor()
        {
            //시네머신 데이터 핸들에 데이터 할당 
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            if (playerFeatureData)
            {
                CinemachineData[] cmData = playerFeatureData.PlayerCinemachineData;
                if (cmData == null)
                {
                    return;
                }

                for (int i = 0; i < cmData.Length; i++)
                {
                    _playerCmDataMediator.ChangeDataInHandle(gameObject, cmData[i]);
                }
            }
;       }
        
        public PlayerFeature GetPlayerFeatureWithProperty(PlayerFeatureProperty featureProperty)
        {
            bool bGet = _dicPlayerFeatures.TryGetValue((int)featureProperty, out PlayerFeature playerFeature);
            if(!bGet)
            {
                Debug.LogError($"Error: _dicPlayerFeatures has not {featureProperty}");
            }
            return playerFeature;
        }

        public AudioSFXTarget GetAudioSFXTarget(PlayerSFXAuidoProperty sfxProperty)
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

        public Animator PlayerAnimator => _playerAnimator;
        public AnimatorData PlayerAnimatorData => _playerData.AnimatorData;
        public PlayerData PlayerData => _playerData;
        public PlayerSkin CurrentPlayerSkin => _curPlayerSkin;
        public LayerMask TargetLayerMask => _targetLayerMask;

    }
}