using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;

namespace KoiAI.Camera
{
    /// <summary>
    /// 시네머신 데이터 핸들들을 가져와서 커넥터와 핸들을 중재하는 클래스
    /// </summary>
    public class CinemachineDataMediator : MonoBehaviour
    {
        [InfoBox("아래 Connect 버튼을 누르면 핸들과 커넥터를 중재하여 데이터를 할당해줍니다.", EInfoBoxType.Normal)]
        [HorizontalLine(5, EColor.Gray)]
        
        [Header("시네머신 커넥터")]
        [SerializeField]
        private CinemachineDataConnector _cmDataConnector;
        [Header("시네머신 핸들")]
        [SerializeField]
        private List<CinemachineDataHandle> _cmDataHandles = new();

        [Button("Connect Handles In Connector")]
        public void ConnectHandlesInConnector()
        {
            _cmDataConnector.ConnectHandles(_cmDataHandles);
        }

        public void ChangeDataInHandle(GameObject controller, CinemachineData data)
        {
            Type cmType = data.GetCinemachineType();
            CinemachineDataHandle cmDataHandle = GetCinemachineDataHandle(cmType);
            if (!cmDataHandle)
            {
                bool bAdd = TryAddCinemachineDataHandle(controller);
                cmDataHandle = GetCinemachineDataHandle(cmType);
                if (!bAdd || !cmDataHandle)
                {
                    Debug.Log("Failed Change Data In Handle: CinemachineDataHandle is null");
                    return;
                }
            }
            cmDataHandle.SetData(data);
        }

        private bool TryAddCinemachineDataHandle(GameObject controller)
        {
            CinemachineDataHandle[] cmDataHandles = controller.GetComponents<CinemachineDataHandle>();
            if(cmDataHandles == null || cmDataHandles.Length <= 0)
            {
                return false;
            }
            for(int i = 0; i < cmDataHandles.Length; i++)
            {
                CinemachineDataHandle cmDataHandle = cmDataHandles[i];
                if (!_cmDataHandles.Contains(cmDataHandle))
                {
                    _cmDataHandles.Add(cmDataHandle);
                }
            }
            return true;
        }

        private CinemachineDataHandle GetCinemachineDataHandle(Type cmType)
        {
            for (int i = 0; i < _cmDataHandles.Count; i++)
            {
                Type cmTypeToCompare = _cmDataHandles[i].Data.GetCinemachineType();
                if (cmTypeToCompare == cmType)
                {
                    return _cmDataHandles[i];
                }
            }
            return null;
        }
    }
}
