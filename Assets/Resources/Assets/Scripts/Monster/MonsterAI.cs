using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public abstract class MonsterFeature : MonoBehaviour
{
    public MonsterAI Owner { get; set; }
    public virtual void Init() { }
    public abstract void EnterFeature();
    public abstract void UpdateFeature();
    public abstract void ExitFeature();
}

[Serializable]
public struct MonsterFeatureHandler
{
    [Header("변경 전 Feature")]
    [SerializeField]
    private MonsterFeature _fromFeature;
    [Header("변경 후 Feature")]
    [SerializeField]
    private MonsterFeature _toFeature;
    public readonly MonsterFeature ToFeature => _fromFeature;
    public readonly MonsterFeature FromFeature => _toFeature;
}

public enum MonsterState
{
    Detection,
    Attack,
}

public class MonsterAI : MonoBehaviour
{
    [SerializeField]
    private MonsterFeatureHandler[] _allFeaturesHandler;
    [SerializeField]
    private MonsterFeature _curMonsterFeature;

    private Dictionary<ulong, MonsterFeatureHandler> _dicFeatureHanlder;

    private void Awake()
    {
        _dicFeatureHanlder = new();
    }

    private void Start()
    {
        InitFeatures();
        if (_curMonsterFeature != null)
        {
            _curMonsterFeature.EnterFeature();
        }
    }

    private void Update()
    {
        if (_curMonsterFeature != null)
        {
            _curMonsterFeature.UpdateFeature();
        }
    }

    private void InitFeatures()
    {
        for (int i = 0; i < _allFeaturesHandler.Length; i++)
        {
            EntityId featureID = _allFeaturesHandler[i].FromFeature.GetEntityId();
            ulong instanceID = EntityId.ToULong(featureID);
            _dicFeatureHanlder.Add(instanceID, _allFeaturesHandler[i]);
            _allFeaturesHandler[i].FromFeature.Owner = this;
            _allFeaturesHandler[i].FromFeature.Init();
        }
    }
    
    /// <summary>
    /// 변경 후 Feature 구하기
    /// </summary>
    private bool TryGetToFeature(MonsterFeature fromFeature, out MonsterFeature toFeature)
    {
        EntityId featureID = fromFeature.GetEntityId();
        ulong instanceID = EntityId.ToULong(featureID);
        if(_dicFeatureHanlder.TryGetValue(instanceID, out var featureHandler))
        {
            toFeature = featureHandler.ToFeature;
            return true;
        }
        toFeature = null;
        return false;
        
    }

    /// <summary>
    /// 변경 후 Feature로 바꾸기
    /// </summary>
    public void ChangeFeature(MonsterFeature callerFeature)
    {
        if (callerFeature != null)
        {
            bool bGet = TryGetToFeature(callerFeature, out MonsterFeature toFeature);
            if(bGet)
            {
                callerFeature.ExitFeature();
                _curMonsterFeature = toFeature;
                _curMonsterFeature.EnterFeature();
            }
        }
    }
}
