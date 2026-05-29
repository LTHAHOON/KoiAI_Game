using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;


[RequireComponent(typeof(Collider))]
public class GravityControl : MonoBehaviour
{
    [SerializeField]
    private float _distance = 100f;
    [SerializeField]
    private LayerMask _groundLayerMask;
    [SerializeField]
    private float _gravity = -9.81f;
    private float _curTime;
    private float _startTime;
    private float _curSpeed = 0f;
    private float _startY = 0f;
    private bool _isGrounded = true;
    private bool _isForce = true;
    private float _resistance = 0f;
    [SerializeField]
    private float _threshold = 0.1f;

    void Update()
    {
        if(!_isForce)
        {
            if (IsGround())
            {
                _curSpeed = 0f;
                _curTime = 0f;
                _startTime = Time.time;
                _startY = transform.position.y;
                _resistance = 0f;
                return;
            }
        }
        
        if(_resistance > 0)
        {
            _resistance += _gravity * Time.deltaTime;
            _resistance = Mathf.Clamp(_resistance, 0, 100f);
        }
        Vector3 pos = transform.position;
        _curTime = Time.time - _startTime;
        _curSpeed = ((_gravity + _resistance) * _curTime);
        float avgSpeed = (_curSpeed * 0.5f * _curTime);
        pos.y = avgSpeed + _startY;
        transform.position = pos;
    }

    public void AddResistanceForce(float resistance)
    {
        _resistance = resistance;
        _isForce = true;
    }

    public bool IsGround()
    {
        _isGrounded = Physics.Raycast(transform.position, transform.up * -1, _distance, _groundLayerMask);
        return _isGrounded;
    }

    public void OnCollisionEnter(Collision collision)
    {
        _isForce = false;
    }

    public bool IsGrounded => _isForce;
}
