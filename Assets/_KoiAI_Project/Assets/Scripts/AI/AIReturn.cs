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
            Brain.AgentController.StopMovement();
      
            if(Brain.AIAnimator)
            {
                Brain.AIAnimator.SetBool(_animParamData.WalkParmID, true);
            }
        }

        public override void UpdateFeature()
        {
            Brain.AgentController.MoveToDest(Brain.OriginPosition, _valueData.MoveSpeed + _extensionData.MoveSpeedMod);
        }

        public override void ExitFeature()
        {
            if(Brain.AIAnimator)
            {
                Brain.AIAnimator.SetBool(_animParamData.WalkParmID, false);
            }
        }
    }
}
