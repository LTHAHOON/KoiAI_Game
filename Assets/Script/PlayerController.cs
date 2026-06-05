using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerFeature;

public abstract class PlayerFeature : MonoBehaviour 
{
    //아이템에서 접근을 가능하게 해주는 Key역할을 합니다.
    public enum PlayerFeatureProperty
    {
        None,
        Equipment,
    }
    public virtual PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.None;
    public PlayerController Owner { get; set; }
    public virtual void Init(PlayerInputAction playerIA) { }
    public abstract void UpdateFeature();
}

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private List<PlayerFeature> _playerFeatures;
    [SerializeField]
    private InventorySystem _myInventorySystem;

    private readonly Dictionary<int, PlayerFeature> _dicPlayerFeatures = new();
    private PlayerInput _playerInput;
    private PlayerInputAction _playerInputAction;
    private void Awake()
    {
        //PlayerInput 바인딩
        _playerInput = GetComponent<PlayerInput>();
        _playerInputAction = new();
        _playerInput.actions = _playerInputAction.asset;
    }
    private void Start()
    {
        for (int i = 0; i < _playerFeatures.Count; i++)
        {
            _playerFeatures[i].Owner = this;
            _playerFeatures[i].Init(_playerInputAction);
            PlayerFeatureProperty featureProperty = _playerFeatures[i].FeatureProperty;
            if (featureProperty != PlayerFeatureProperty.None)
            {
                _dicPlayerFeatures.Add((int)featureProperty, _playerFeatures[i]);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < _playerFeatures.Count; i++)
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

    public InventorySystem MyInventorySystem => _myInventorySystem
    public PlayerInputAction PlayerIA=> _playerInputAction;
}
