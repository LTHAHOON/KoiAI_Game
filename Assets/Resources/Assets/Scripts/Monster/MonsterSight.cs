using UnityEngine;

public class MonsterSight : MonsterFeature
{
    [SerializeField]
    private bool _bUseGizmos = false;
    [SerializeField]
    private float _detectionDistance = 10f;
    [SerializeField]
    private int _detectMaxCount = 1;
    [SerializeField]
    private float _sightAngle = 60f;
    [SerializeField]
    private float _sightDelayTime = 0.5f;
    [SerializeField]
    private LayerMask _targetLayerMask;

    private float _curSightTime = 0f;
    private bool _isFindPlayer = false;
    private GameObject _target;
    private Collider[] _targetColliders;

    public override MonsterState State => MonsterState.Detection;
    public override void Init()
    {
        _targetColliders = new Collider[_detectMaxCount];
    }
    public override void EnterFeature()
    {

    }
    
    public override void UpdateFeature()
    {
        if(_target == null)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, _detectionDistance, _targetColliders, _targetLayerMask);
            if (count <= 0)
            {
                return;
            }
            _target = _targetColliders[_targetColliders.Length - 1].gameObject;
        }

        float distance = (_target.transform.position - transform.position).sqrMagnitude;
        if (_curSightTime < _sightDelayTime)
        {
            if(distance >= _detectionDistance * _detectionDistance)
            {
                _isFindPlayer = false;
                return;
            }
            _curSightTime += Time.deltaTime;
            return;
        }
        Vector3 dirToPlayer = _target.transform.position - transform.position;
        dirToPlayer.Normalize();
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        _isFindPlayer = angle < _sightAngle;
        _target = angle < _sightAngle ? _target : null;
        _curSightTime = 0;
       
    }

    public override void ExitFeature()
    {
    }

    public GameObject GetTargetToFind()
    {
        return _isFindPlayer ? _target : null;
    }
    public void OnDrawGizmos()
    {
        if (_bUseGizmos)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _detectionDistance);
        }
    }
    public bool IsFindTarget() => _isFindPlayer;

}
