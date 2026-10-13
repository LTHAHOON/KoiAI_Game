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
            SetMovementAnimation(Vector3.zero, 0f);
        }

        public override void UpdateFeature()
        {
            float maxMoveSpeed = _valueData.MoveSpeed + _extensionData.MoveSpeedMod;

            Brain.AgentController.MoveToDest(Brain.OriginPosition, maxMoveSpeed);

            SetMovementAnimation(Brain.AgentController.CurrentMoveVelocity, maxMoveSpeed);
        }

        public override void ExitFeature()
        {
            Brain.AgentController.StopMovement_Force();
            if (_animParamData && Brain.AIAnimator)
            {
                _animParamData.StopMovementParametersAsync(Brain.AIAnimator, Brain.BeginMovementAnimationStop())
                    .SuppressCancellationThrow().Forget();
            }

        }

        private void SetMovementAnimation(Vector3 velocity, float maxMoveSpeed = 1f)
        {
            Brain.CancelMovementAnimationStop();
            if (_animParamData)
            {
                Vector3 localVelocity = Brain.transform.InverseTransformDirection(velocity);
                _animParamData.SetMovementParameters(Brain.AIAnimator, localVelocity, maxMoveSpeed);
            }
        }
    }
}
