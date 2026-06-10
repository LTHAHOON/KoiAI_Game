using System;
using UnityEngine;

[RequireComponent(typeof(MonsterSight))]
[RequireComponent(typeof(SurfaceAngleFinder))]
public class MonsterRotation : MonoBehaviour
{
    [SerializeField]
    private float _lookSpeed = 10f;
    
    private SurfaceAngleFinder _surfaceAngleFinder;
    private MonsterSight _monsterSight;
    private Vector3 _targetAngle = Vector3.zero;

    private void Awake()
    {
        _monsterSight = GetComponent<MonsterSight>();
        _surfaceAngleFinder = GetComponent<SurfaceAngleFinder>();
    }
    
    private void Update()
    {
        _surfaceAngleFinder.TrySurfaceAngleUsingRaycast(out _targetAngle, transform);
        bool isFindPlayer = _monsterSight.IsFindPlayer();
        if (isFindPlayer)
        {
            GameObject target = _monsterSight.GetPlayerToFind();
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            _targetAngle.y = angle;
        }
        else
        {
            _targetAngle.y = transform.eulerAngles.y;
        }
        Quaternion quat = Quaternion.Euler(_targetAngle.x, _targetAngle.y, _targetAngle.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, quat, Time.deltaTime * _lookSpeed);    
    }
}
