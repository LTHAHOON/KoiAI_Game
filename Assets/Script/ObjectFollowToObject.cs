using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectFollowToObject : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransform;
    [SerializeField]
    private float _delayTime;
    [SerializeField]
    private float _followSpeed;
    [SerializeField]
    private float _offsetYThresold;

    private Vector3 _myPos;
    private float _curTime = 0f;
    private Vector3 _offset = Vector3.zero;
    private void Awake()
    {
        _myPos = _targetTransform.position;
    }

    void FixedUpdate()
    {
        transform.position = _myPos;
        if ((_targetTransform.position - transform.position).sqrMagnitude <= 0.001f)
        {
            _offset.y = 0f;
            _curTime = 0f;
            return;
        }

        if (_curTime < _delayTime)
        {
            _curTime += Time.fixedDeltaTime;
            return;
        }

        _offset.y = (_targetTransform.position.y - _myPos.y) * _offsetYThresold;
        _myPos = Vector3.Lerp(_myPos, _targetTransform.position + _offset, _followSpeed * Time.fixedDeltaTime);

    }
}
