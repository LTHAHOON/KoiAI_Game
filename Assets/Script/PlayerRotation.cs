using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : PlayerFeature
{
    [SerializeField]
    private float _surfaceCheckDistance = 3f;
    [SerializeField]
    private SurfaceAngleFinder _surfaceAngleFinder;
    [SerializeField]
    private float _lookSpeed = 10f;
    private Vector2 _input = Vector2.zero;
    private Camera _camera;
    public override void Init(PlayerInputAction playerIA)
    {
        _camera = Camera.main;
        playerIA.Player.Move.started += OnRotation;
        playerIA.Player.Move.performed += OnRotation;
        playerIA.Player.Move.canceled += OnRotation;
    }

    public override void UpdateFeature()
    {
        if(_input == Vector2.zero)
            return;
        _surfaceAngleFinder.TrySurfaceAngleUsingRaycast(out Vector3 angleVec, transform, _surfaceCheckDistance);
        float angleY = GetAngleWithAtan(_input);
        Quaternion quat = Quaternion.Euler(angleVec.x, angleY, angleVec.z);
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
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float crossY = Vector3.Cross(transform.forward, dir).y;
        //0~180도를 -180~180도로 확장
        if(crossY < 0f)
            angle = -angle;
        
        return angle + transform.eulerAngles.y;
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
