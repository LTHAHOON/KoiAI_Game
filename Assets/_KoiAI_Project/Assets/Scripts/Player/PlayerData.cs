using NaughtyAttributes;
using UnityEngine;
using static KoiAI.Player.PlayerFeature;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.Player
{
    using KoiAI.AnimatorSystem;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [CreateAssetMenu(fileName = "new PlayerData", menuName = "KoiAI/Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [SerializeField]
        private string _chracterBaseName;
        [ReadOnly]
        [SerializeField]
        private PlayerFeatureDataBase _playerFeatureDataBase;
        [Space(10)]
        [SerializeField]
        private PlayerFeatureDataType _playerFeatureDataType;
        [SerializeField]
        private PlayerSkin _playerSkin;

        [Space(10)]
        [HorizontalLine(5, EColor.Gray)]
        [Space(10)]
        [SerializeField]
        private AnimatorData _animatorData;

        [Space(10)]
        [ShowIf(nameof(HasMovementProperty))]
        [SerializeField]
        private PlayerMovementExtensionData _playerMovementExtensionData;
        [ShowIf(nameof(HasRotationProperty))]
        [SerializeField]
        private PlayerRotationExtensionData _playerRotationExtensionData;

        [HideInInspector]
        [SerializeField]
        private List<Guid> _wearingCostumeGUIDs = new();
        [HideInInspector]
        private List<Guid> _lastWearingCostumeGUIDs = new();

        [ReadOnly]
        [SerializeField]
        private Vector2 _curColorPosition_Face;
        [ReadOnly]
        [SerializeField]
        private Vector2 _curColorPosition_Body;

        [HideInInspector]
        [SerializeField]
        private Vector2 _lastColorPosition_Face;
        [HideInInspector]
        [SerializeField]
        private Vector2 _lastColorPosition_Body;

        public void SaveWearingCostumeGUIDs()
        {
            _lastWearingCostumeGUIDs = _wearingCostumeGUIDs.ToList();
        }


        public List<Guid> GetWearingCostumeGUIDs()
        {
            return _wearingCostumeGUIDs;
        }

        public List<Guid> GetLastWearingCostumeGUIDs()
        {
            return _lastWearingCostumeGUIDs;
        }

        public void SetCurColorPositionFace(Vector2 faceColorPos)
        {
            _curColorPosition_Face = faceColorPos;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void SetCurColorPositionBody(Vector2 bodyColorPos)
        {
            _curColorPosition_Body = bodyColorPos;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        public void SetLastColorPosition()
        {
            _lastColorPosition_Face = _curColorPosition_Face;
            _lastColorPosition_Body = _curColorPosition_Body;

        }

        public void UndoToLastColorPosition()
        {
            _curColorPosition_Face = _lastColorPosition_Face;
            _curColorPosition_Body = _lastColorPosition_Body;
        }

        public PlayerFeatureData GetPlayerFeatureData()
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
                    PlayerFeatureProperty.Equipment => data.PlayerEquipmentValueData,
                    _ => null
                };
            }
            return null;

            
        }
        

        public bool HasMovementProperty => GetPlayerFeatureData() is var data && data != null && data.HasMovementProperty;
        public bool HasRotationProperty => GetPlayerFeatureData() is var data && data != null && data.HasRotationProperty;
        
        public Vector2 CurColorPosition_Face => _curColorPosition_Face;
        public Vector2 CurColorPosition_Body => _curColorPosition_Body;
        public string CharacterBaseName => _chracterBaseName;
        public PlayerFeatureDataType PlayerFeatureDataType => _playerFeatureDataType;
        public PlayerSkin PlayerSkin => _playerSkin;
        public AnimatorData AnimatorData => _animatorData;
    }
}
