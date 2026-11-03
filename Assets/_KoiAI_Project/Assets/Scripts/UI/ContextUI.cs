using System;
using UnityEngine;

namespace KoiAI.UI
{
    public enum ContextUIType
    {
        HotKey,
    }
    
    public class ContextUI : MonoBehaviour
    {
        [Serializable]
        public struct ContextUIData
        {
            [SerializeField]
            private ContextUIType _contextUIType;
            [SerializeField]
            private GameObject _contextUIObj;

            public readonly ContextUIType ContextUIType => _contextUIType;
            public readonly GameObject ContextUIObj => _contextUIObj;
        }

        [SerializeField]
        private ContextUIData[] _contextUIData;

        private static Func<ContextUIType, ContextUIData?> OnGetContextUIData;
        
        private void Awake()
        {
            OnGetContextUIData = GetContextUIData;
        }

        public static void Show(ContextUIType contextUIType)
        {
            ContextUIData? contextUIData = OnGetContextUIData?.Invoke(contextUIType);
            if(!contextUIData.HasValue || !contextUIData.Value.ContextUIObj)
            {
                Debug.Log("Error: ContextUIData를 불러오지 못했습니다.");
                return;
            }
            contextUIData.Value.ContextUIObj.SetActive(true);
        }

        public static void Hide(ContextUIType contextUIType)   
        {
            ContextUIData? contextUIData = OnGetContextUIData?.Invoke(contextUIType);
            if(!contextUIData.HasValue || !contextUIData.Value.ContextUIObj)
            {
                Debug.Log("Error: ContextUIData를 불러오지 못했습니다.");
                return;
            }
            contextUIData.Value.ContextUIObj.SetActive(false);
        }

        public void OnDestroy()
        {
            OnGetContextUIData -= GetContextUIData;
        }

        private ContextUIData? GetContextUIData(ContextUIType contextUIType)
        {
            for (int i = 0; i < _contextUIData.Length; ++i)
            {
                if(_contextUIData[i].ContextUIType != contextUIType)
                {
                    continue;
                }
                return _contextUIData[i];
            }
            return null;
        }
    }
}
