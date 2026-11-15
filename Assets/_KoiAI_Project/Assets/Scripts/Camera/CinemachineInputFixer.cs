using System.Collections;
using KoiAI.Input;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.KoiCamera
{
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class CinemachineInputFixer : MonoBehaviour
    {
        private CinemachineInputAxisController _axisController;
        private InputActionReference _runtimeLookActionReference;

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

            _runtimeLookActionReference = InputActionReference.Create(InputService.PlayerIA.Player.Look);

            _axisController.enabled = false;

            int connectedControllerCount = 0;
            foreach (var controller in _axisController.Controllers)
            {
                InputActionReference actionReference = controller.Input.InputAction;
                InputAction action = actionReference != null ? actionReference.action : null;
                if (action != null && action.name == "Look" && action.actionMap.name == "Player")
                {
                    controller.Input.InputAction = _runtimeLookActionReference;
                    connectedControllerCount++;
                }
            }

            _axisController.enabled = true;
        }

        private void OnDestroy()
        {
            if (_runtimeLookActionReference != null)
            {
                Destroy(_runtimeLookActionReference);
            }
        }
    }
}
