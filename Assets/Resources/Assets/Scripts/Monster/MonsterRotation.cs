using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EntitySight))]
public class MonsterRotation : MonsterFeature
{
    [SerializeField]
    private float _lookSpeed = 10f;
    [SerializeField]
    private float _surfaceCheckDistance = 3f;
    [Header("Rotation 최소 거리")]
    [Tooltip("Feature 변경할 탐색 거리")]
    [SerializeField]
    private float _detectDistanceToFeature;

    private SurfaceAngleFinder _surfaceAngleFinder;
    private EntitySight _entitySight;
    private Vector3 _targetAngle = Vector3.zero;

    public override void Init()
    {
        _entitySight = GetComponent<EntitySight>();
        _surfaceAngleFinder = new(_surfaceCheckDistance);
    }

    public override void EnterFeature()
    {
        if (!_entitySight || _surfaceAngleFinder == null)
        {
            Owner.ChangeFeature(this);
        }
    }

    public override void UpdateFeature()
    {
        _entitySight.Detect();
        _surfaceAngleFinder.TryGetLocalSurfaceAngle(out _targetAngle, transform);
        bool isFindPlayer = _entitySight.IsFindTarget();
        if (isFindPlayer)
        {
            GameObject target = _entitySight.GetTargetToFind();
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            _targetAngle.y = angle;
            if(dir.sqrMagnitude <= _detectDistanceToFeature * _detectDistanceToFeature)
            {
                Owner.ChangeFeature(this);
                return;
            }
        }
        else
        {
            _targetAngle.y = transform.eulerAngles.y;
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
