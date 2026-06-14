using System;
using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    private CannonBallData _cannonBallData;
    [SerializeField]
    private LayerMask _targetLayerMask;
    
    private Collider[] _colliders;
    
    public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
    {
        _cannonBallData = cannonBallData;
        _targetLayerMask = targetLayerMask;
        _colliders = new Collider[_cannonBallData.MaxOverlapCount];
    }
       
    public void OnTriggerEnter(Collider other)
    {
        if (_cannonBallData == null)
        {
            return;
        }
        if (_colliders == null)
        {
            _colliders = new Collider[_cannonBallData.MaxOverlapCount];
        }
        if (other.CompareTag(GameTags.Ground))
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position,_cannonBallData.RadiusExplosion, _colliders, _targetLayerMask);
            if (count > 0)
            {
                Debug.Log("데미지를 주었습니다.");
                gameObject.SetActive(false);
                enabled = false;
            }

        }
    }
}
