using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 10f;
    private Vector3 _moveDir;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (_moveDir.sqrMagnitude > 0)
        {
            Vector3 xDir = _camera.transform.right * _moveDir.x;
            Vector3 zDir = _camera.transform.forward * _moveDir.z;
            Vector3 dir = (xDir + zDir).normalized;
            dir.y = 0f;
            Vector3 translation = _moveSpeed * Time.deltaTime * dir;
            transform.Translate(translation, Space.World);
        }
        /*
        if(Keyboard.current is not null)
        {

            float xMove = 0;
            float zMove = 0;
            if(Keyboard.current.aKey.isPressed)
            {
                xMove = -1f;
            }
            if(Keyboard.current.dKey.isPressed)
            {
                xMove = 1f;
            }
            if(Keyboard.current.wKey.isPressed)
            {
                zMove = 1f;
            }
            if(Keyboard.current.sKey.isPressed)
            {
                zMove = -1f;
            }
            Vector3 dir = new Vector3(xMove, 0f, zMove);
            dir.Normalize();
            if (dir.magnitude > 0)
            {
                Vector3 translation = _moveSpeed * Time.deltaTime * dir;
                transform.Translate(translation, Space.World);
            }
                
        }
        */
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
    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            Debug.Log("Mouse Click");
        }
    }
}
