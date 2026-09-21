using NaughtyAttributes;
using UnityEngine;
using static KoiAI.AI.AIFeature;

namespace KoiAI.AI
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Core;

    public class AIStatData : EntityData
    {
        [SerializeField]
        private string _aiBaseName;

        [Space(10)]
        [HorizontalLine(5, EColor.Gray)]
        [Space(10)]
        [SerializeField]
        private AnimatorData _animatorData;
        
        [SerializeField]
        private AIFeatureDataBase _aiFeatureDataBase;
        [Space(10)]
        [SerializeField]
        private AIFeatureDataType _aiFeatureDataType;
        
        [Space(10)]
        [ShowIf(nameof(HasMovementProperty))]
        [SerializeField]
        private AIMovementExtensionData _aiMovementExtensionData;
        [ShowIf(nameof(HasRotationProperty))]
        [SerializeField]
        private AIRotationExtensionData _aiRotationExtensionData;
        [ShowIf(nameof(HasAttackProperty))]
        [SerializeField]
        private AIAttackExtensionData _aiAttackExtensionData;

        public AIFeatureData GetAIFeatureData()
        {
            AIFeatureData data = _aiFeatureDataBase?.GetAIFeatureData(_aiFeatureDataType);
            return data;
        }

        public AIFeatureExtensionData GetAIFeatureExtensionData(AIFeatureProperty featureProperty)
        {
            return featureProperty switch
            {
                AIFeatureProperty.Movement => _aiMovementExtensionData,
                AIFeatureProperty.Return => _aiMovementExtensionData,
                AIFeatureProperty.Rotation => _aiRotationExtensionData,
                AIFeatureProperty.Attack => _aiAttackExtensionData,
                _ => null
            };
        }

        public AIFeatureValueData GetAIFeatureValueData(AIFeatureProperty featureProperty)
        {
            AIFeatureData data = GetAIFeatureData();
            if (data)
            {
                return featureProperty switch
                {
                    AIFeatureProperty.Movement => data.AIMovementValueData,
                    AIFeatureProperty.Return => data.AIMovementValueData,
                    AIFeatureProperty.Rotation => data.AIRotationValueData,
                    _ => null
                };
            }
            return null;
        }

        public bool HasMovementProperty => GetAIFeatureData() is var data && data != null && data.HasMovementProperty;
        public bool HasRotationProperty => GetAIFeatureData() is var data && data != null && data.HasRotationProperty;
        public bool HasAttackProperty => GetAIFeatureData() is var data && data != null && data.HasAttackProperty;


        public AnimatorData AnimatorData => _animatorData;
    }
}
