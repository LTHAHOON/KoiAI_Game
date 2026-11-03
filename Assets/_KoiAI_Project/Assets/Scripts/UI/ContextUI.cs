using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoiAI.Utilities;
using TMPro;
using UnityEngine;

namespace KoiAI.UI
{
    public enum ContextPromptUIType
    {
        InteractPrompt,
        LockedInteractPrompt,
    }
    
    public class ContextUI : MonoBehaviour
    {
        [Serializable]
        public struct ContextUIData
        {
            [SerializeField]
            private ContextPromptUIType _contextUIType;
            [SerializeField]
            private CanvasGroup _contextUICanvasGroup;
            [SerializeField]
            private UIFollowToObject _contextUIObj;
            [SerializeField]
            private TMP_Text _descriptionText;
            [SerializeField]
            private TMP_Text _hotKeyText;

            public readonly ContextPromptUIType ContextUIType => _contextUIType;
            public readonly CanvasGroup ContextUICanvasGroup => _contextUICanvasGroup;
            public readonly UIFollowToObject ContextUIObj => _contextUIObj;
            public readonly TMP_Text DescriptionText => _descriptionText;
            public readonly TMP_Text HotKeyText => _hotKeyText;
        }

        [SerializeField]
        private ContextUIData[] _contextUIData;

        private static Func<ContextPromptUIType, ContextUIData?> OnGetContextUIData;
        
        private void Awake()
        {
            OnGetContextUIData = GetContextUIData;
        }

        public static void Show(ContextPromptUIType contextUIType, GameObject targetObject, string description, string keyText = null, bool bUseAlphaSmooth = true)
        {
            ContextUIData? contextUIData = OnGetContextUIData?.Invoke(contextUIType);
            if(!contextUIData.HasValue || !contextUIData.Value.ContextUIObj)
            {
                Debug.Log("Error: ContextUIData를 불러오지 못했습니다.");
                return;
            }
            if(contextUIData.Value.HotKeyText && !string.IsNullOrEmpty(keyText))
            {
                contextUIData.Value.HotKeyText.text = keyText;
            }
            contextUIData.Value.DescriptionText.text = description;
            contextUIData.Value.ContextUIObj.SetTargetObject(targetObject);
            if(contextUIData.Value.ContextUICanvasGroup)
            {
                contextUIData.Value.ContextUICanvasGroup.alpha = 0f;
            }
            if(bUseAlphaSmooth)
            {
                SetCanvasGroupAlpha(contextUIData.Value.ContextUICanvasGroup, 1f, 0.05f);
            }
            else
            {
                contextUIData.Value.ContextUICanvasGroup.alpha = 1f;
                contextUIData.Value.ContextUICanvasGroup.gameObject.SetActive(true);
            }
        }

        public static void Hide(ContextPromptUIType contextUIType)
        {
            ContextUIData? contextUIData = OnGetContextUIData?.Invoke(contextUIType);
            if(!contextUIData.HasValue || !contextUIData.Value.ContextUIObj)
            {
                Debug.Log("Error: ContextUIData를 불러오지 못했습니다.");
                return;
            }

            SetCanvasGroupAlpha(contextUIData.Value.ContextUICanvasGroup, 0f, 0.2f);
        }

        private static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if(canvasGroup == null)
            {
                canvasGroup.gameObject.SetActive(targetAlpha > 0f);
                return;
            }
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, targetAlpha, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    canvasGroup.gameObject.SetActive(targetAlpha > 0f);
                });
        }

        public void OnDestroy()
        {
            OnGetContextUIData -= GetContextUIData;
        }

        private ContextUIData? GetContextUIData(ContextPromptUIType contextUIType)
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
