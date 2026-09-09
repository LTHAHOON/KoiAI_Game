using UnityEngine;

namespace KoiAI.AI
{
    using KoiAI.AnimatorSystem;

    public class AIIdle : AIFeature
    {

        private AnimatorParamData _animParamData;
        public override void InitFeature(AIFeatureValueData aiFeatureValueData = null, AIFeatureExtensionData aiFeatureExtensionData = null)
        {
            
            if (Brain.AIAnimatorData.IsValid())
            {
                _animParamData = Brain.AIAnimatorData.AnimParamData;
            }
        }

        public override void EnterFeature()
        {
            Brain.AIAnimator.SetBool(_animParamData.IdleParmID, true);
        }

        public override void ExitFeature()
        {
            Brain.AIAnimator.SetBool(_animParamData.IdleParmID, false);
        }

        public override void UpdateFeature()
        {
  
        }

      
    }
}
