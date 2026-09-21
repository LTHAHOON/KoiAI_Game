using System;
using System.Linq;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static KoiAI.Player.PlayerFeature;

namespace KoiAI.Player
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Core;

    [CreateAssetMenu(fileName = "new PlayerData", menuName = "KoiAI/Player/PlayerData")]
    public class PlayerData : EntityData, ISaveDTOHandler
    {
        private PlayerDTO _playerDto;
        public void SetFromSaveDTO(SaveDTO saveDTO)
        {
            if (saveDTO is PlayerDTO playerDto)
            {
                _playerDto = playerDto;
                _wearingCostumeGUIDs = _playerDto.WearingCostumeGUIDs;
                _curColor_Face = _playerDto.CurColor_Face;
                _curColor_Body = _playerDto.CurColor_Body;
            }
        }

        public void SetToSaveDTO()
        {
            if (_playerDto == null)
            {
                //완전 처음 게임을 시작할 경우
                _playerDto = new PlayerDTO(_curColor_Face, _curColor_Body, _wearingCostumeGUIDs);
            }
            else
            {
                _playerDto.SetPlayerDTO(_curColor_Face, _curColor_Body, _wearingCostumeGUIDs);
            }
        }

        public SaveDTO GetSaveDTO()
        {
            return _playerDto;
        }
        
        [SerializeField]
        private string _chracterBaseName;
        [SerializeField]
        private long _characterID;
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
        
        public long CharacterID => _characterID;

        private List<Guid> _wearingCostumeGUIDs = new();
        private List<Guid> _lastWearingCostumeGUIDs = new();
        
        private Color _curColor_Face;
        private Color _curColor_Body;
        
        private Color _lastColor_Face;
        private Color _lastColor_Body;

        public void Initialize()
        {
            _wearingCostumeGUIDs.Clear();
            _lastWearingCostumeGUIDs.Clear();
            _curColor_Face = Color.white;
            _curColor_Body = Color.white;
            _playerDto = null;
        }
        
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

        public void SetCurColorFace(Color faceColor)
        {
            _curColor_Face = faceColor;
        }

        public void SetCurColorBody(Color bodyColor)
        {
            _curColor_Body = bodyColor;
        }
        
        public void SetLastColor()
        {
            _lastColor_Face = _curColor_Face;
            _lastColor_Body = _curColor_Body;
        }

        public void UndoToLastColor()
        {
            _curColor_Face = _lastColor_Face;
            _curColor_Body = _lastColor_Body;
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

        public Color CurColor_Face => _curColor_Face;
        public Color CurColor_Body => _curColor_Body;
        public string CharacterBaseName => _chracterBaseName;
        public PlayerFeatureDataType PlayerFeatureDataType => _playerFeatureDataType;
        public PlayerSkin PlayerSkin => _playerSkin;
        public AnimatorData AnimatorData => _animatorData;

    }
}
