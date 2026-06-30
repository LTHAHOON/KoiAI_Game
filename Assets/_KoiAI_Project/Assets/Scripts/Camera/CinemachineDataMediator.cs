using System;
using UnityEngine;
using NaughtyAttributes;

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
        private CinemachineDataHandle[] _cmDataHandles;

        [Button("Connect Handles In Connector")]
        public void ConnectHandlesInConnector()
        {
            _cmDataConnector.ConnectHandles(_cmDataHandles);
        }

        public void ChangeDataInHandle(CinemachineData data)
        {
            CinemachineDataHandle cmDataHandle = GetCinemachineDataHandle(data.GetCinemachineType());
            if (cmDataHandle == null)
            {
                Debug.Log("Failed Change Data In Handle: CinemachineDataHandle is null");
                return;
            }
            cmDataHandle.SetData(data);
        }

        private CinemachineDataHandle GetCinemachineDataHandle(Type cmType)
        {
            for (int i = 0; i < _cmDataHandles.Length; i++)
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
