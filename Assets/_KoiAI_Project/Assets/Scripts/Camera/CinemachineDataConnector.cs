using Unity.Cinemachine;
using UnityEngine;
using System;

namespace KoiAI.Camera
{
    public class CinemachineDataConnector : MonoBehaviour
    {
        [Header("시네머신 카메라")]
        [SerializeField]
        private CinemachineCamera _cmCamera;


        [Header("현재 시네머신 컴포넌트")]
        [SerializeField]
        private CinemachineComponentBase[] _cmComponents;

        public void ConnectHandles(CinemachineDataHandle[] cmDataHandles)
        {
            if(cmDataHandles == null || cmDataHandles.Length <= 0)
            {
                Debug.Log("Failed Connect Handle: CinemachineDataHandles Length <= 0");
            }
            bool isComplete = false;
            for (int i = 0; i < cmDataHandles.Length; i++)
            {
                CinemachineDataHandle cmDataHandle = cmDataHandles[i];

                Type type = cmDataHandle.Data.GetCinemachineType();
                CinemachineCore.Stage stage = cmDataHandle.Data.GetCinemachineStage();
                bool bGet = TryGetCinemachineComponent(out var cmComponent, type, stage);
                if(bGet)
                {
                    cmDataHandle.SetDataInComponent(cmComponent);
                    isComplete = true;
                }
            }
            if (isComplete)
            {
                Debug.Log("Completed Connect Handle");
            }
        }

        private bool TryGetCinemachineComponent(out CinemachineComponentBase component, Type type, CinemachineCore.Stage stage)
        {
            for (int i = 0; _cmComponents.Length > i; i++)
            {
                CinemachineComponentBase cmComponent = _cmComponents[i];
                if (cmComponent.Stage == stage && cmComponent.GetType() == type)
                {
                    component = cmComponent;
                    return true;
                }
            }
            component = default;
            return false;
        }
    }
}
