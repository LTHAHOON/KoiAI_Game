using NaughtyAttributes;
using UnityEngine;
using static KoiAI.Player.PlayerFeature;
namespace KoiAI.Player
{
    [CreateAssetMenu(fileName = "new PlayerData", menuName = "KoiAI/Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [ReadOnly]
        [SerializeField]
        private PlayerFeatureDataBase _playerFeatureDataBase;
        [SerializeField]
        private PlayerFeatureDataType _playerFeatureDataType;
        [SerializeField]
        private PlayerSkin _playerSkin;

        [ShowIf(nameof(HasMovementProperty))]
        [SerializeField]
        private PlayerMovementExtensionData _playerMovementExtensionData;
        [ShowIf(nameof(HasRotationProperty))]
        [SerializeField]
        private PlayerRotationExtensionData _playerRotationExtensionData;
        
        
        private PlayerFeatureData GetPlayerFeatureData()
        {
            PlayerFeatureData data = _playerFeatureDataBase?.GetPlayerFeatureData(_playerFeatureDataType);
            return data;
        }

        public PlayerFeatureExtensionData GetPlayerFeatureExtensionData(PlayerFeatureProperty featureProperty)
        {
            return featureProperty switch
            {
                PlayerFeatureProperty.Movement => _playerMovementExtensionData,
                PlayerFeatureProperty.Rotation => _playerRotationExtensionData,
                _ => null
            };
        }
        
        public PlayerFeatureValueData GetPlayerFeatureValueData(PlayerFeatureProperty featureProperty)
        {
            PlayerFeatureData data = GetPlayerFeatureData();
            if (data)
            {
                return featureProperty switch
                {
                    PlayerFeatureProperty.Movement => data.PlayerMovementValueData,
                    PlayerFeatureProperty.Rotation => data.PlayerRotationValueData,
                    PlayerFeatureProperty.WayPoint => data.PlayerWayPointValueData,
                    _ => null
                };
            }
            return null;
        }
        
        public bool HasMovementProperty => GetPlayerFeatureData() is var data && data != null && data.HasMovementProperty;
        public bool HasRotationProperty => GetPlayerFeatureData() is var data && data != null && data.HasRotationProperty;
        
        public PlayerFeatureDataType PlayerFeatureDataType => _playerFeatureDataType;
        public PlayerSkin PlayerSkin => _playerSkin;
    }
}
