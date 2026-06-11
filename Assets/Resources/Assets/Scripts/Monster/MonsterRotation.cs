using System;
using UnityEngine;

[RequireComponent(typeof(MonsterSight))]
public class MonsterRotation : MonsterFeature
{
    [SerializeField]
    private float _lookSpeed = 10f;
    [SerializeField]
    private float _surfaceCheckDistance = 3f;

    private SurfaceAngleFinder _surfaceAngleFinder;
    private MonsterSight _monsterSight;
    private Vector3 _targetAngle = Vector3.zero;
    public override MonsterState State => MonsterState.Detection;
    public override void Init()
    {
        _monsterSight = GetComponent<MonsterSight>();
        _surfaceAngleFinder = new(_surfaceCheckDistance);
    }

    public override void EnterFeature()
    {
    }

    public override void UpdateFeature()
    {
        if(!_monsterSight || _surfaceAngleFinder == null)
        {
            return;
        }

        Vector3 localForward = transform.InverseTransformDirection(transform.forward);
        _surfaceAngleFinder.TryGetLocalSurfaceAngle(out _targetAngle, transform);
        bool isFindPlayer = _monsterSight.IsFindTarget();
        if (isFindPlayer)
        {
            GameObject target = _monsterSight.GetTargetToFind();
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            _targetAngle.y = angle;
        }

        Quaternion quat = Quaternion.Euler(_targetAngle.x, _targetAngle.y, _targetAngle.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, quat, Time.deltaTime * _lookSpeed);
    }

    public override void ExitFeature()
    {
    }


  
    
    private void Update()
    {
 
    }
}
