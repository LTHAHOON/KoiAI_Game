using System;
using UnityEngine;
using NaughtyAttributes;
using Unity.Cinemachine;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.Camera
{
    /// <summary>
    /// 시네머신 데이터 핸들들을 가져와서 커넥터와 핸들을 중재하는 클래스
    /// </summary>
    public class CinemachineDataModerator : MonoBehaviour
    {
        [Header("시네머신 핸들")]
        [SerializeField]
        private CinemachineDataHandle[] _cmDataHandles;
        [Header("시네머신 커넥터")]
        [SerializeField]
        private CinemachineDataConnector _cmDataConnector;

#if UNITY_EDITOR
        [Button("Connect Handles In Connector")]
        public void ConnectHandlesInConnector()
        {
            _cmDataConnector.ConnectHandles(_cmDataHandles);
        }
#endif
    }
}
