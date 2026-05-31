using System;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;


[RequireComponent(typeof(Collider))]
public class GravityControl : MonoBehaviour
{
    [SerializeField]
    private float _groundCheckDistance = 1.2f;
    [SerializeField]
    private LayerMask _groundLayerMask;
    [SerializeField]
    private float _gravity = -9.81f;
    [SerializeField] 
    private float _gravityThresold = 10f;
    
    private float _groundCheckDist;
    private float _curTime;
    private float _startTime;
    private float _curSpeed = 0f;
    private float _startY = 0f;
    private bool _isGrounded = false;
    private float _initialVelocity = 0f;
    private float gravity;
    private void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        if (IsGround())
        {
            if (!_isGrounded)
            {
                Init();
            }
            _initialVelocity = 0f;
            _startY = transform.position.y;
            _isGrounded = true;
            _startTime = Time.time;
            return;
        }

        _isGrounded = false;
        GravityProcess();
    }

    private void Init()
    {
        gravity = _gravity;
        _curSpeed = 0f;
        _curTime = 0f;
        _groundCheckDist = _groundCheckDistance;
        _startY = transform.position.y;
        _startTime = Time.time;
    }
    private void GravityProcess()
    {
        if (_curSpeed < 0)
        {
            gravity -= Time.deltaTime * _gravityThresold;
        }
        else if(_curSpeed > 0)
        {
            _groundCheckDist = _groundCheckDistance;
        }
        _curTime = Time.time - _startTime;
        Vector3 pos = transform.position;
        float avgSpeed = (gravity * 0.5f * _curTime * _curTime) + _initialVelocity * _curTime;
        pos.y =  avgSpeed + _startY;
        transform.position = pos;
        _curSpeed = _initialVelocity + gravity * _curTime;
    }
    
    public void AddResistanceForce(float resistance)
    {
        _initialVelocity = resistance;
        _startTime = Time.time;
        _startY = transform.position.y;
        _groundCheckDist = 0f;
    }

    private bool IsGround()
    {
        return Physics.Raycast(transform.position, transform.up * -1, _groundCheckDist);
    }
    
    public bool IsGrounded => _isGrounded;
}
