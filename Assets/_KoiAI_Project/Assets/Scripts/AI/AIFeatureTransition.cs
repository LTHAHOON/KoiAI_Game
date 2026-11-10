using NaughtyAttributes;
using Cysharp.Text;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using static KoiAI.AI.AIFeature;
using static KoiAI.AI.AIFeatureTransition;

namespace KoiAI.AI
{
    [Serializable]
    public class AIFeatureTransitionRuntimeDebug
    {
        [ReadOnly]
        [AllowNesting]
        public string DEBUG_STATE;

        private bool _wasActive = false;

        public void AssignDebug(AIFeature feature)
        {
            string featureName = feature.FeatureProperty.ToString();
            string enterState = ZString.Format("{0} {1}", featureName, nameof(AIFeatureState.ENTER));
            string updateState = ZString.Format("{0} {1}", featureName, nameof(AIFeatureState.UPDATE));
            string exitState = ZString.Format("{0} {1}", featureName, nameof(AIFeatureState.EXIT));

            Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate)
                .Subscribe(_ =>
                {
                    bool isActive = feature.Brain.IsFeatureActive(feature.FeatureProperty);
                    if (isActive && !_wasActive)
                    {
                        DEBUG_STATE = enterState;
                    }
                    else if (isActive && _wasActive)
                    {
                        DEBUG_STATE = updateState;
                    }
                    else
                    {
                        DEBUG_STATE = exitState;
                    }
                    _wasActive = isActive;
                })
                .AddTo(feature.Brain);
        }
    }

    [Serializable]
    public class AIFeatureTransitionRuntimeSettings
    {
        [Serializable]
        public struct AIFeatureTransitionRuntimeSetting
        {
            [SerializeField]
            private AIFeatureProperty _featureProperty;

            [SerializeField]
            private float _enableDelayTime;
            [SerializeField]
            private float _disableDelayTime;

            public AIFeatureProperty FeatureProperty => _featureProperty;
            public float EnableDelayTime => _enableDelayTime;
            public float DisableDelayTime => _disableDelayTime;
        }

        [SerializeField] 
        private List<AIFeatureTransitionRuntimeSetting> _runtimeSettings;

        private bool TryGetRuntimeSetting(out AIFeatureTransitionRuntimeSetting runtimeSetting, AIFeature feature)
        {
            for (int i = 0; i < _runtimeSettings.Count; i++)
            {
                if (_runtimeSettings[i].FeatureProperty == feature.FeatureProperty)
                {
                    runtimeSetting = _runtimeSettings[i];
                    return true;
                }
            }
            runtimeSetting = default;
            return false;
        }
        
        public float GetEnableDelayTime(AIFeature feature)
        {
            bool bGet = TryGetRuntimeSetting(out AIFeatureTransitionRuntimeSetting runtimeSetting, feature);
            if (bGet)
            {
                return runtimeSetting.EnableDelayTime;
            }
            return 0f;
        }
        
        public float GetDisableDelayTime(AIFeature feature)
        {
            bool bGet = TryGetRuntimeSetting(out AIFeatureTransitionRuntimeSetting runtimeSetting, feature);
            if (bGet)
            {
                return runtimeSetting.DisableDelayTime;
            }
            return 0f;
        }

    }

    [Serializable]
    public struct AIFeatureTransition
    {
        public enum AIFeatureTransitionType
        {
            None,
            HasTarget,
            Distance,
            WithFeature
        }

        public enum AIFeatureState
        {
            ENTER,
            UPDATE,
            EXIT
        }

        [SerializeField]
        private AIFeatureProperty _featureProperty;

        [SerializeField]
        private AIConditionGroupData _enableConditions;

        [SerializeField]
        private AIConditionGroupData _disableConditions;


        public AIFeatureProperty FeatureProperty => _featureProperty;
        public AIConditionGroupData EnableConditions => _enableConditions;
        public AIConditionGroupData DisableConditions => _disableConditions;
    }
}
