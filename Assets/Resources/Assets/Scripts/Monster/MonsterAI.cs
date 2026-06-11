using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public abstract class MonsterFeature : MonoBehaviour
{
    public MonsterAI Owner { get; set; }
    public abstract MonsterState State { get; }
    public virtual void Init() { }
    public abstract void EnterFeature();
    public abstract void UpdateFeature();
    public abstract void ExitFeature();
}

public enum MonsterState
{
    Detection,
    Attack,
}

public class MonsterAI : MonoBehaviour
{
    [SerializeField]
    private MonsterFeature[] _allMonsterFeatures;
    [SerializeField]
    private MonsterState _startMonsterState;

    private List<MonsterFeature> _curMonsterFeatures;

    private void Awake()
    {
        for (int i = 0; i < _allMonsterFeatures.Length; i++)
        {
            _allMonsterFeatures[i].Owner = this;
            _allMonsterFeatures[i].Init();
        }
    }

    private void Start()
    {
        _curMonsterFeatures = FindFeatures(_startMonsterState);
        if(_curMonsterFeatures != null)
        {
            EnterCurrentFeatures();
        }
    }

    private void Update()
    {
        if (_curMonsterFeatures != null)
        {
            UpdateCurrentFeatures();
        }
    }

    public void EnterCurrentFeatures()
    {
        for (int i = 0; i < _curMonsterFeatures.Count; i++)
        {
            _curMonsterFeatures[i].EnterFeature();
        }
    }

    public void UpdateCurrentFeatures()
    {
        for (int i = 0; i < _curMonsterFeatures.Count; i++)
        {
            _curMonsterFeatures[i].UpdateFeature();
        }
    }

    public void ExitCurrentFeatures()
    {
        for (int i = 0; i < _curMonsterFeatures.Count; i++)
        {
            _curMonsterFeatures[i].ExitFeature();
        }
    }


    private List<MonsterFeature> FindFeatures(MonsterState targetState)
    {
        List<MonsterFeature> monsterFeatures = null;
        for (int i = 0; i < _allMonsterFeatures.Length; i++)
        {
            if(_allMonsterFeatures[i].State == targetState)
            {
                if(monsterFeatures == null)
                {
                    monsterFeatures = new();
                }
                monsterFeatures.Add(_allMonsterFeatures[i]);
            }
        }
        return monsterFeatures;
    }

    public void ChangeFeature(MonsterFeature callerFeature, MonsterState targetState)
    {
        var monsterFeatures = FindFeatures(targetState);

        if (monsterFeatures != null)
        {
            ExitCurrentFeatures();
            _curMonsterFeatures = monsterFeatures;
        }
    }
}
