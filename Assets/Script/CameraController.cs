using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private Vector3 _armOffset;
    [Range(5, 20)]
    [SerializeField]
    private float _distance;
    [SerializeField]
    private float _sensity;
    [SerializeField]
    private float _maxPitch;
    [SerializeField]
    private float _minPitch;

    private float _mouseX;
    private float _mouseY;
    private Vector2 _mouseDelta;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void LateUpdate()
    {
        _mouseX += _mouseDelta.x * _sensity * Time.deltaTime;
        _mouseY += _mouseDelta.y * _sensity * Time.deltaTime;
        _mouseY = Mathf.Clamp(_mouseY, _minPitch, _maxPitch);
        Quaternion targetRot = Quaternion.Euler(_mouseY, _mouseX, 0f);
        Vector3 targetCenter = _player.position + _armOffset;
        Vector3 targetPos = targetCenter + ((targetRot * Vector3.back) * _distance);
        transform.position = targetPos;

        Quaternion lookQuat = Quaternion.LookRotation(targetCenter - transform.position);
        transform.rotation = lookQuat;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }
}
