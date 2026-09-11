using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KoiAI.AI.AIFeature;

namespace KoiAI.AI
{
    using KoiAI.Health;
    using KoiAI.AnimatorSystem;
    using KoiAI.Nav;
    
    public abstract class AIFeatureExtensionData { }
    public abstract class AIFeatureValueData { }

    /// <summary>
    /// AI 두뇌(판단) 클래스
    /// </summary>
    public class AIBrain : MonoBehaviour
    {
        [SerializeField]
        private AISightConditionData _sightConditionData;
        private AISightCondition _sightCondition;

        [SerializeField]
        private bool _useDebugState = true;
        [SerializeField]
        private NavigationController _agentController;
        [SerializeField]
        private AIStatData _aiStatData;
        [SerializeField]
        private AIBrainData _aiBrainData;
        [ReadOnly]
        [SerializeField]
        private List<AIFeatureTransitionRuntimeDebug> _aiRuntimeDebugs;
        [SerializeField]
        private AIFeatureTransitionRuntimeSettings _aiRuntimeSettings;

        private readonly AITargetContext _targetContext = new();
        private Action[] _aiDecisionLogics;
        private int _aiDecisionLogicIndex = -1;
        private List<AIFeature> _aiFeatures;
        private Dictionary<AIFeatureProperty, Func<AIFeature>> _dicAIFeatureCreator;
        private readonly HashSet<AIFeatureProperty> _activeFeatures = new();
        private readonly HashSet<AIFeatureProperty> _transitioningFeatures = new();
        private Vector3 _originPosition;
        private Animator _aiAnimator;
        private Health _health;
        
        public void AwakeAIBrain()
        {
            _originPosition = transform.position;
            _sightCondition = new AISightCondition(_sightConditionData);

            _aiFeatures = new();
            _dicAIFeatureCreator = new()
            {
                { AIFeatureProperty.Idle, () => new AIIdle()},
                { AIFeatureProperty.Movement, () => new AIMovement()},
                { AIFeatureProperty.Rotation, () => new AIRotation()},
                { AIFeatureProperty.Attack, () => new AIAttack()},
                { AIFeatureProperty.Return, () => new AIReturn()}

            };
            _aiDecisionLogics = new Action[]
            {
                DecisionDisableLogic,
                DecisionEnableLogic,
            };
            TryGetComponent(out _aiAnimator);
            TryGetComponent(out _health);
        }
        
        public void StartAIBrain()
        {
            InitFeatures();
            StartCoroutine(AIDecisionLogic());
        }

        public void UpdateAIBrain()
        {
            _sightCondition.Tick(this);

            //SightCondition 결과를 Context에 복사
            UpdateTargetContext();

            UpdateActiveFeature();
        }

        private void UpdateTargetContext()
        {
            GameObject target = _sightCondition.Target;

            if (target)
            {
                _targetContext.SetTarget(transform, target.transform);
            }
            else
            {
                _targetContext.Clear();
            }
        }

        private void InitFeatures()
        {
            for (int i = 0; i < _aiBrainData.FeatureTransitions.Count; i++)
            {
                AIFeatureTransition transition = _aiBrainData.FeatureTransitions[i];
                AIFeatureProperty property = transition.FeatureProperty;
                if (_activeFeatures.Contains(property))
                {
                    continue;
                }

                AIFeatureValueData valueData = _aiStatData.GetAIFeatureValueData(property);
                AIFeatureExtensionData extensionData = _aiStatData.GetAIFeatureExtensionData(property);

                if (_dicAIFeatureCreator.TryGetValue(property, out Func<AIFeature> featureCreator))
                {
                    AIFeature aiFeature = featureCreator?.Invoke();
                    if (aiFeature == null)
                    {
                        continue;
                    }
                    aiFeature.Brain = this;
                    aiFeature.Init(transition, valueData, extensionData);

                    if(_useDebugState)
                    {
                        _aiRuntimeDebugs.Add(new());
                        _aiRuntimeDebugs[i].AssignDebug(aiFeature);
                    }
                    _activeFeatures.Add(property);
                    _aiFeatures.Add(aiFeature);
                }
            }
            _activeFeatures.Clear();
        }

        private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
        private IEnumerator AIDecisionLogic()
        {
            while (true)
            {
                NextAIDecisionLogic();
                yield return _waitForSeconds0_1;
            }
        }

        private void NextAIDecisionLogic()
        {
            ++_aiDecisionLogicIndex;
            _aiDecisionLogicIndex = _aiDecisionLogicIndex % _aiDecisionLogics.Length;
            _aiDecisionLogics[_aiDecisionLogicIndex]?.Invoke();
        }

        /// <summary>
        /// Feature 비활성화 판단 로직
        /// </summary>
        private void DecisionDisableLogic()
        {
            foreach (AIFeature feature in _aiFeatures)
            {
                if (_activeFeatures.Contains(feature.FeatureProperty))
                {
                    if(feature.CheckDisable())
                    {
                        DisableFeature(feature).Forget();
                    }
                }
            }
        }

        /// <summary>
        /// Feature 활성화 판단 로직
        /// </summary>
        private void DecisionEnableLogic()
        {
            foreach (AIFeature feature in _aiFeatures)
            {
                if (!_activeFeatures.Contains(feature.FeatureProperty))
                {
                    if (feature.CheckEnable())
                    {
                        EnableFeature(feature).Forget();
                    }
                }
            }
        }

        /// <summary>
        /// 활성화된 Feature 업데이트 로직
        /// </summary>
        private void UpdateActiveFeature()
        {
            foreach (AIFeature feature in _aiFeatures)
            {
                if (_activeFeatures.Contains(feature.FeatureProperty))
                {
                    feature.UpdateFeature();
                }
            }
        }

        private async UniTask EnableFeature(AIFeature feature)
        {
            if (_activeFeatures.Contains(feature.FeatureProperty)
                || !_transitioningFeatures.Add(feature.FeatureProperty))
            {
                return;
            }

            try
            {
                feature.EnterFeature();

                float delayTime = _aiRuntimeSettings.GetEnableDelayTime(feature);
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: destroyCancellationToken);

                _activeFeatures.Add(feature.FeatureProperty);
            }
            finally
            {
                _transitioningFeatures.Remove(feature.FeatureProperty);
            }
        }

        public async UniTask DisableFeature(AIFeature feature)
        {
            if (!_activeFeatures.Contains(feature.FeatureProperty)
                || !_transitioningFeatures.Add(feature.FeatureProperty))
            {
                return;
            }

            try
            {
                feature.ExitFeature();

                float delayTime = _aiRuntimeSettings.GetDisableDelayTime(feature);
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: destroyCancellationToken);

                _activeFeatures.Remove(feature.FeatureProperty);
            }
            finally
            {
                _transitioningFeatures.Remove(feature.FeatureProperty);
            }
        }

        public bool IsFeatureActive(AIFeatureProperty property)
        {
            return _activeFeatures.Contains(property);
        }


        private void OnDrawGizmosSelected()
        {
            if (_sightConditionData == null || !_sightConditionData.UseGizmos)
            {
                return;
            }

            Transform eye = _sightConditionData.EyeTransform ? _sightConditionData.EyeTransform : transform;

            Gizmos.color = _sightConditionData.GizmosColor;

            Gizmos.DrawWireSphere(eye.position, _sightConditionData.DetectionDistance);

            Gizmos.DrawWireSphere(eye.position, _sightConditionData.LoseDistance);
        }

        public bool IsDead => _health && _health.IsDead;
        public AITargetContext TargetContext => _targetContext;
        public NavigationController AgentController => _agentController;
        public AnimatorData AIAnimatorData => _aiStatData.AnimatorData;
        public Animator AIAnimator => _aiAnimator;
        public Vector3 OriginPosition => _originPosition;
    }
}
