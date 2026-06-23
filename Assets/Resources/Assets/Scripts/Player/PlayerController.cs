using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerFeature;
using static PlayerSFXAudioFeature;

public abstract class PlayerFeature : MonoBehaviour 
{
    //아이템에서 접근을 가능하게 해주는 Key역할을 합니다.
    public enum PlayerFeatureProperty
    {
        None,
        Rotation,
        Equipment,
    }
    public virtual PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.None;
    public PlayerController Owner { get; set; }
    public virtual void Init(PlayerInputAction playerIA) { }
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
    private PlayerFeature[] _playerFeatures;
    [SerializeField]
    private PlayerSFXAudioFeature[] _sfxFeatures;
    [SerializeField]
    private LayerMask _targetLayerMask;
    
    private readonly Dictionary<int, PlayerFeature> _dicPlayerFeatures = new();
    private PlayerInput _playerInput;
    private PlayerInputAction _playerInputAction;
    private void Awake()
    {
        //PlayerInput 바인딩
        _playerInput = GetComponent<PlayerInput>();
        _playerFeatures = GetComponents<PlayerFeature>();
        _playerInputAction = new();
        _playerInput.actions = _playerInputAction.asset;
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
            _playerFeatures[i].Init(_playerInputAction);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _playerFeatures.Length; i++)
        {
            _playerFeatures[i].UpdateFeature();
        }
    }

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

    public PlayerInputAction PlayerIA=> _playerInputAction;
    public LayerMask TargetLayerMask => _targetLayerMask;
}
