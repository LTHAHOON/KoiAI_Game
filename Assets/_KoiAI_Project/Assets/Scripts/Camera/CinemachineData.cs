using System;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.KoiCamera
{
    public abstract class CinemachineData : ScriptableObject
    {
        public abstract Type GetCinemachineType();
        public abstract CinemachineCore.Stage GetCinemachineStage();
    }
}
