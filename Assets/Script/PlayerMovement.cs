using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 10f;
    private Vector3 moveDir;
    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            return;
        }
        Vector2 normalizedDir = context.ReadValue<Vector2>();
        moveDir = new Vector3(normalizedDir.x, 0f, normalizedDir.y);
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            Debug.Log("Mouse Click");
        }
    }
    void Update()
    {
        if (moveDir.sqrMagnitude > 0)
        {
            Vector3 translation = _moveSpeed * Time.deltaTime * moveDir;
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

}
