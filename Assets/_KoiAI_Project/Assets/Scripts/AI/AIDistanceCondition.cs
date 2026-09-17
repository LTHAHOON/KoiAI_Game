using System;
using KoiAI.Utilities;
using UnityEngine;
using static KoiAI.AI.AIFeatureTransition;

namespace KoiAI.AI
{
    [Serializable]
    public class AIDistanceConditionData
    {
        [SerializeField]
        private bool _useOriginDistance;
        [SerializeField]
        private CompareValueCondition<float> _distanceCondition;
                
        public bool UseOriginDistance => _useOriginDistance;
        public CompareValueCondition<float> DistanceCondition
            => _distanceCondition;
    }
    public class AIDistanceCondition : AITransitionCondition
    {
        private readonly AIDistanceConditionData _conditionData;

        public AIDistanceCondition(
            AIDistanceConditionData conditionData)
        {
            
            _conditionData = conditionData;
        }

        public override AIFeatureTransitionType TransitionType => AIFeatureTransitionType.Distance;

        public override bool Check(AIBrain brain)
        {
            if (_conditionData == null || brain == null)
            {
                return false;
            }

            bool result = false;
            if (_conditionData.UseOriginDistance)
            {
                Vector3 originPosition = brain.OriginPosition;

                if(brain.AgentController && brain.AgentController.TryGetPathDistance(originPosition, out float pathDistance))
                {
                    result = pathDistance.CompareWithCondition(_conditionData.DistanceCondition);
                }
                else
                {
                    originPosition.y = 0f;
                    Vector3 curPosition = brain.transform.position;
                    curPosition.y = 0f;
                    float distance = (originPosition - curPosition).sqrMagnitude;
                    result = distance.CompareWithCondition(_conditionData.DistanceCondition);
                }
            }
            else
            {
                if (!brain.TargetContext.HasTarget)
                {
                    return false;
                }
                if(brain.AgentController && brain.AgentController.TryGetPathDistance(brain.TargetContext.Target.position, out float pathDistance))
                {
                    result   = pathDistance.CompareWithCondition(_conditionData.DistanceCondition);
                }
                else
                {
                    result   = brain.TargetContext.Distance.CompareWithCondition(_conditionData.DistanceCondition);
                }
            }


            return result;
        }
    }
}
