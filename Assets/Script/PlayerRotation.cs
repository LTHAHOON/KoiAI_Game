using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField]
    private float _targetMoveSpeed = 3f;
    [SerializeField]
    private float _lookSpeed = 10f;
    [SerializeField]
    private float _targetDistance;
    private Vector2 _input = Vector2.zero;
    private Vector3 targetPos = Vector3.zero;
    
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if(_input == Vector2.zero)
            return;
        float angle = GetAngleWithAtan(_input);
        Quaternion quat = Quaternion.Euler(0f, angle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, quat, Time.deltaTime * _lookSpeed);
        /*
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        _input = Vector2.zero;
        if (Keyboard.current.qKey.isPressed)
        {
            _input.x -= 1f;
        }
        if(Keyboard.current.eKey.isPressed)
        {
            _input.x += 1f;
        }
        targetPos += new Vector3(_input.x, _input.y, 0f) * _targetMoveSpeed * Time.deltaTime;
        targetPos.z = _targetDistance;
        Vector3 targetDirection = targetPos.normalized;
        Quaternion lookQuat = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookQuat, Time.deltaTime * _lookSpeed);
        */
    }

    private float GetAngleWithAtan(Vector2 input)
    {
        Vector3 xDir = _camera.transform.right * input.x;
        Vector3 zDir = _camera.transform.forward * input.y;
        Vector3 dir = (xDir + zDir).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        return angle;
    }
    
    public void OnRotation(InputAction.CallbackContext context)
    {
        if (context.started)
            return;
        _input = context.ReadValue<Vector2>();
    }
}
