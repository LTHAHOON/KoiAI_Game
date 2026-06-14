using NUnit.Framework.Internal;
using UnityEngine;

[RequireComponent (typeof(EntitySight))]
public class MonsterAttack : MonsterFeature
{
    [Header("몬스터 무기")]
    [SerializeField]
    private ActivateRandomValue<WeaponControllerBase>[] _randomWeaponContorllers;
    [Header("Attack 최대 거리")]
    [Tooltip("Feature 변경할 탐색 거리")]
    [SerializeField]
    private float _detectDistanceToFeature;
    [SerializeField]
    private float _attackDelayTime = 1f;

    private EntitySight _entitySight;
    private GameObject _target;
    private float _curAttackTime = 0f;
    public override void Init()
    {
        _entitySight = GetComponent<EntitySight>();
        for (int i = 0; i < _randomWeaponContorllers.Length; i++)
        {
            _randomWeaponContorllers[i].ActivateTarget.Init();
        }
    }

    public override void EnterFeature()
    {
        _target = _entitySight.GetTargetToFind();
        if(!_target)
        {
            Owner.ChangeFeature(this);
        }
    }

    public override void UpdateFeature()
    {
        _entitySight.Detect();
        WeaponControllerBase weaponController = ActivateRandom.GetRandomActivateTarget(_randomWeaponContorllers);
        if (!_entitySight.IsFindTarget())
        {
            Owner.ChangeFeature(this);
            weaponController.EndAiming();
            return;
        }
        Vector3 dir = _target.transform.position - transform.position;
        if (dir.sqrMagnitude >= _detectDistanceToFeature * _detectDistanceToFeature)
        {
            Owner.ChangeFeature(this);
            weaponController.EndAiming();
            return;
        }
        
        if(_curAttackTime < _attackDelayTime )
        {
            _curAttackTime += Time.deltaTime;
            return;
        }
        _curAttackTime = 0f;
        
         weaponController.TryGetYawPitch(_target.transform.position,out float yaw, out float pitch);
         weaponController.StartAiming(pitch, yaw);
         weaponController.Activate();
    }

    public override void ExitFeature()
    {

    }
}
