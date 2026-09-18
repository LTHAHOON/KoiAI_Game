using Cysharp.Threading.Tasks;
using KoiAI.AnimatorSystem;
using UnityEngine;

namespace KoiAI.AI
{
    public class AIReturn : AIFeature
    {
        private AIMovementValueData _valueData;
        private AnimatorParamData _animParamData;
        private AIMovementExtensionData _extensionData;
        public override void InitFeature(AIFeatureValueData enemyFeatureValueData = null, AIFeatureExtensionData enemyFeatureExtensionData = null)
        {
            if (enemyFeatureValueData is not AIMovementValueData valueData
                || enemyFeatureExtensionData is not AIMovementExtensionData extensionData)
            {
                return;
            }
            _valueData = valueData;
            _extensionData = extensionData;
            if (Brain.AIAnimatorData.IsValid())
            {
                //애니메이터 파라미터 데이터 초기화
                _animParamData = Brain.AIAnimatorData.AnimParamData;
            }
            else
            {
                Debug.Log("Check: EnemyAnimatorData is not valid.");
            }
        }

        public override void EnterFeature()
        {
            Brain.AgentController.StopMovement_Force();
        }

        public override void UpdateFeature()
        {
            float maxMoveSpeed = _valueData.MoveSpeed + _extensionData.MoveSpeedMod;

            Brain.AgentController.MoveToDest(Brain.OriginPosition, maxMoveSpeed);

            if (Brain.AIAnimator)
            {
                float curMoveSpeed = Brain.AgentController.CurrentMoveSpeed;
                curMoveSpeed = Mathf.Clamp01(curMoveSpeed / maxMoveSpeed);
                Brain.AIAnimator.SetFloat(_animParamData.WalkParmID, curMoveSpeed);
            }
        }

        public override void ExitFeature()
        {
            Brain.AgentController.StopMovement_Force();
        }
    }
}
