using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.Camera
{
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class CinemachineInputFixer : MonoBehaviour
    {
        private CinemachineInputAxisController _axisController;

        private void Awake()
        {
            _axisController = GetComponent<CinemachineInputAxisController>();
        }

        private void OnEnable()
        {
            if (!_axisController)
            {
                return;
            }

            // 씬 로드 직후 타이밍 이슈를 방지하기 위해 코루틴으로 한 프레임 쉽니다.
            StartCoroutine(ReactiveInputsRoutine());
        }

        private IEnumerator ReactiveInputsRoutine()
        {
            yield return null;

            _axisController.enabled = false;
            _axisController.enabled = true;

            //등록된 모든 컨트롤러를 돌면서 새로고침
            foreach (var controller in _axisController.Controllers)
            {
                if (controller.Input.InputAction)
                {
                    var action = controller.Input.InputAction.action;
                    if (action != null)
                    {
                        action.Disable();
                        action.Enable();
                    }
                }
            }
        }
    }
}
