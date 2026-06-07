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
    private Vector3 _targetOffset = Vector3.zero;
    [Tooltip("점프 시 푹 들어가는 현상을 방지해줍니다.")]
    [Header("타겟 위로 뜰 때 조정값")]
    [SerializeField]
    private float _followYPosThresold = 0.5f;
    private Vector3 _myPos;
    private float _curTime = 0f;
    private void Awake()
    {
        _myPos = _targetTransform.position;
    }

    void FixedUpdate()
    {
        if (_targetTransform == null)
        {
            return;
        }
        Vector3 targetPos = _targetTransform.position + _targetOffset;
        
        if ((targetPos -_myPos).sqrMagnitude <= 0.001f)
        {
            transform.position = _myPos;
            _curTime = 0f;
            return;
        }

        if (_curTime < _delayTime)
        {
            _curTime += Time.fixedDeltaTime;
            transform.position = _myPos;
            return;
        }

        _myPos = Vector3.Lerp(_myPos,targetPos, _followSpeed * Time.fixedDeltaTime);
        if (_targetTransform.position.y *  _followYPosThresold > _myPos.y)
        {
            _myPos.y = _targetTransform.position.y *  _followYPosThresold ;
        }
        transform.position = _myPos;
    }
}
