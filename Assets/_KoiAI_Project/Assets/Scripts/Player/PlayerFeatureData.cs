using System;
using KoiAI.Camera;
using NaughtyAttributes;
using UnityEngine;
using static KoiAI.Player.PlayerFeature;

namespace KoiAI.Player
{
    [Serializable]
    public enum PlayerFeatureDataType
    {
        Small, //소형 캐릭터
        Medium, //중형 캐릭터
        Large, //대형 캐릭터
        Static, //움직이지 않는 캐릭터
    }
    
    [CreateAssetMenu(fileName = "new PlayerFeatureData", menuName = "KoiAI/Player/PlayerFeatureData")]
    public class PlayerFeatureData : ScriptableObject
    {
        [SerializeField]
        private PlayerFeatureDataType _playerFeatureDataType;
        [SerializeField]
        private PlayerFeatureProperty[] _properties;

        [Space(10)]
        [SerializeField]
        private CinemachineData[] _playerCinemachineData;
        [ShowIf(nameof(HasMovementProperty))]
        [SerializeField]
        private PlayerMovementValueData _playerMovementValueData;
        [ShowIf(nameof(HasRotationProperty))]
        [SerializeField]
        private PlayerRotationValueData _playerRotationValueData;
        [ShowIf(nameof(HasWayPointProperty))]
        [SerializeField]
        private PlayerWayPointValueData _playerWayPointValueData;
        
        private bool HasProperty(PlayerFeatureProperty property)
        {
            bool bHas = Array.IndexOf(_properties, property) != -1;
            return bHas;
        }
        
        public PlayerMovementValueData PlayerMovementValueData => _playerMovementValueData;
        public PlayerRotationValueData PlayerRotationValueData => _playerRotationValueData;
        public PlayerWayPointValueData PlayerWayPointValueData => _playerWayPointValueData; 
        public bool HasMovementProperty => HasProperty(PlayerFeatureProperty.Movement);
        public bool HasWayPointProperty => HasProperty(PlayerFeatureProperty.WayPoint);
        public bool HasRotationProperty => HasProperty(PlayerFeatureProperty.Rotation);
        public PlayerFeatureDataType PlayerFeatureDataType => _playerFeatureDataType;
        public PlayerFeatureProperty[] Properties => _properties;
        public CinemachineData[] PlayerCinemachineData => _playerCinemachineData;

    }
}
