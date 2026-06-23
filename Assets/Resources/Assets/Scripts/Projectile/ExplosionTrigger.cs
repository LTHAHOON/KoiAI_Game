using System;
using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask _targetLayerMask;

    private CannonBallData _cannonBallData;
    private Collider[] _colliders;
    private Action<Vector3> OnExplosion;
    
    public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask, Action<Vector3> explosionEvent)
    {
        _cannonBallData = cannonBallData;
        _targetLayerMask = targetLayerMask;
        _colliders = new Collider[_cannonBallData.MaxOverlapCount];
        OnExplosion += explosionEvent;
    }
    
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
        if (_cannonBallData == null)
        {
            return;
        }
        if (_colliders == null)
        {
            _colliders = new Collider[_cannonBallData.MaxOverlapCount];
        }
        int count = Physics.OverlapSphereNonAlloc(transform.position, _cannonBallData.RadiusExplosion, _colliders, _targetLayerMask);
        for (int i = 0; i < count; i++)
        {
            Debug.Log($"{_cannonBallData.Damage} 만큼 데미지를 주었습니다.");
        }
        Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
        OnExplosion?.Invoke(hitPoint);
    }
}
