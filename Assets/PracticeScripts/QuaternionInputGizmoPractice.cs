using UnityEngine;
// UnityEngine.InputSystem은 Unity 6 기준 새 입력 시스템의 키보드, 마우스, 게임패드 입력을 사용하기 위한 네임스페이스입니다.
using UnityEngine.InputSystem;

public class QuaternionInputGizmoPractice : MonoBehaviour
{
    public float rotationSpeed = 4f;
    public float targetMoveSpeed = 3f;
    public float targetDistance = 4f;
    public float targetRange = 3f;

    Vector3 targetOffset = new Vector3(0f, 0f, 4f);

    void Update()
    {
        // Keyboard.current는 Input System에서 현재 키보드 장치를 가져오는 프로퍼티입니다.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        // Vector2.zero는 (0, 0)을 뜻하는 2D 벡터 기본값입니다.
        Vector2 input = Vector2.zero;

        // aKey, leftArrowKey는 각각 A 키와 왼쪽 방향키를 나타내는 입력 버튼입니다.
        // isPressed는 해당 키가 지금 눌려 있는 동안 true가 되는 프로퍼티입니다.
        if (keyboard.qKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        // dKey, rightArrowKey는 각각 D 키와 오른쪽 방향키를 나타냅니다.
        if (keyboard.eKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        // sKey, downArrowKey는 각각 S 키와 아래 방향키를 나타냅니다.
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }

        // wKey, upArrowKey는 각각 W 키와 위 방향키를 나타냅니다.
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        // spaceKey는 스페이스바 입력 버튼입니다.
        // wasPressedThisFrame은 이번 프레임에 막 눌렸을 때만 true가 되는 프로퍼티입니다.
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            targetOffset = new Vector3(0f, 0f, 0f);
        }

        // Time.deltaTime은 이전 프레임에서 현재 프레임까지 걸린 시간입니다. 프레임 속도가 달라도 이동 속도를 일정하게 맞출 때 사용합니다.
        targetOffset += new Vector3(input.x, input.y, 0f) * targetMoveSpeed * Time.deltaTime;
        // Mathf.Clamp는 값을 최소값과 최대값 사이로 제한하는 메서드입니다.
        targetOffset.x = Mathf.Clamp(targetOffset.x, -targetRange, targetRange);
        targetOffset.y = Mathf.Clamp(targetOffset.y, -targetRange, targetRange);
        targetOffset.z = targetDistance;

        // normalized는 벡터의 방향은 유지하고 길이만 1로 만든 값을 돌려주는 프로퍼티입니다.
        Vector3 targetDirection = targetOffset.normalized;

        // Quaternion.LookRotation은 지정한 방향을 바라보는 회전값을 만들어 주는 메서드입니다.
        // Vector3.up은 월드 기준 위쪽 방향인 (0, 1, 0)을 뜻합니다.
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // transform.rotation은 현재 오브젝트의 회전값이고, Quaternion.Slerp는 두 회전 사이를 부드럽게 섞습니다.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        // transform.position은 현재 오브젝트의 월드 위치입니다.
        Vector3 origin = transform.position;
        Vector3 targetPosition = origin + targetOffset;
        Vector3 targetDirection = targetOffset.normalized;

        // Gizmos.color는 이후에 그릴 기즈모 도형의 색상을 지정하는 프로퍼티입니다.
        // Color.yellow는 유니티가 미리 제공하는 노란색 값입니다.
        Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere는 Scene 뷰에 구체를 그려 특정 위치를 표시하는 메서드입니다.
        Gizmos.DrawSphere(targetPosition, 0.15f);

        // Color.blue는 유니티가 미리 제공하는 파란색 값입니다.
        Gizmos.color = Color.blue;
        // Gizmos.DrawLine은 Scene 뷰에 두 점을 잇는 선을 그리는 메서드입니다.
        // transform.forward는 현재 오브젝트가 바라보는 앞 방향입니다.
        Gizmos.DrawLine(origin, origin + transform.forward * targetDistance);

        // Color.red는 유니티가 미리 제공하는 빨간색 값입니다.
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + targetDirection * targetDistance);
    }
}