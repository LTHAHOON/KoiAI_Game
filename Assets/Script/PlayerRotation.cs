using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField]
    private float _lookSpeed = 10f;
    private Vector2 _input = Vector2.zero;
    
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

    }

    private float GetAngleWithAtan(Vector2 input)
    {
        Vector3 xDir = _camera.transform.right * input.x;
        Vector3 zDir = _camera.transform.forward * input.y;
        Vector3 dir = (xDir + zDir).normalized;
        dir.y = 0f;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        return angle;
    }
    private float GetAngleWithDot(Vector2 input)
    {
        Vector3 xDir = _camera.transform.right * input.x;
        Vector3 zDir = _camera.transform.forward * input.y;
        Vector3 dir = (xDir + zDir).normalized;
        dir.y = 0f;


        float dot = Vector3.Dot(transform.forward, dir);
        dot = Mathf.Clamp(dot, -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        Vector3 cross = Vector3.Cross(transform.forward, dir);
        if(cross.z < 0f)
        {
            angle = -angle;
        }

        Debug.Log(angle);
        return angle;
    }

    public void OnRotation(InputAction.CallbackContext context)
    {
        if (context.started)
            return;
        if (context.performed)
        {
            _input = context.ReadValue<Vector2>();
        }
        if(context.canceled)
        {
            _input = Vector3.zero;
        }
            
    }
}
