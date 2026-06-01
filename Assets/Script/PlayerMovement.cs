using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rigidBody;
    [SerializeField]
    private GravityControl _gravityControl;
    [SerializeField]
    private float _moveSpeed = 10f;
    [SerializeField]
    private float _jumpForce = 10f;
    [SerializeField]
    private int _jumpMaxCount = 1;

    private int _jumpCurCount = 1;
    private Vector3 _moveDir;
    private Camera _camera;
    private Vector3 _translation = Vector3.zero;
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_moveDir.sqrMagnitude > 0)
        {
            Vector3 xDir = _camera.transform.right * _moveDir.x;
            Vector3 zDir = _camera.transform.forward * _moveDir.z;
            Vector3 dir = (xDir + zDir).normalized;
            dir.y = 0f;
            _translation = _moveSpeed * dir;
        }

        if (_gravityControl.IsGrounded)
        {
            _jumpCurCount = 1;
        }
    }
    
    private void FixedUpdate()
    {
        if (_moveDir.sqrMagnitude > 0)
        {
            _rigidBody.linearVelocity = _translation;
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            return;
        }
        Vector2 normalizedDir = context.ReadValue<Vector2>();
        _moveDir = new Vector3(normalizedDir.x, 0f, normalizedDir.y);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if(_jumpCurCount < _jumpMaxCount)
            {
                ++_jumpCurCount;
                _gravityControl.AddForceUp(_jumpForce);
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            Debug.Log("Mouse Click");
        }
    }

}
