using KoiAI.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.Camera
{
    public abstract class CinemachineDataHandle : MonoBehaviour 
    {
        [SerializeField]
        private CinemachineData _cmData;
        public abstract void SetDataInComponent(CinemachineComponentBase cmComponent);

        public CinemachineData Data => _cmData;
    }
    
    /// <summary>
    /// 시네머신 데이터 핸들 확장 기반 클래스
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class CinemachineDataHandle<TComp, TData> : CinemachineDataHandle where TComp : CinemachineComponentBase where TData : CinemachineData
    {
        /// <summary>
        /// 사용자 시네머신 데이터를 시네머신 컴포넌트에 넣습니다.
        /// </summary>
        public override void SetDataInComponent(CinemachineComponentBase cmComponent)
        {
            if(cmComponent == null)
            {
                return;
            }
            if(cmComponent is TComp tComponent && Data is TData tData)
            {
                SetDataInComponent(tComponent, tData);
            }
        }

        public abstract void SetDataInComponent(TComp cmComponent, TData cmData);
    }
}
