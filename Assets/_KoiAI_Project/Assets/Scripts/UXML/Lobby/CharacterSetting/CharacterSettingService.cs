using System;
using System.Collections.Generic;
using KoiAI.Costume;
using UnityEngine;

namespace KoiAI.Utilities
{
    using KoiAI.Player;
    using KoiAI.UI;

    public class CharacterSettingService : MonoBehaviour
    {
        private static PlayerData _curPlayerData;
        private static CharacterSettingModel _characterSettingModel;
        private static readonly List<CostumeData> _costumeDataList = new();
        private static int _characterSettingIndex;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void Init(CharacterSettingModel characterSettingModel, int characterSettingIndex)
        {
            _characterSettingModel = characterSettingModel;
            _characterSettingIndex = characterSettingIndex;
        }
        
        public static void AddCostumeData(CostumeData costumeData)
        {
            _costumeDataList.Add(costumeData);
            Debug.Log($"Add CostumeData : {costumeData.CostumeName}");
        }

        public static void RemoveCostumeData(CostumeData costumeData)
        {
            _costumeDataList.Remove(costumeData);
            Debug.Log($"Remove CostumeData : {costumeData.CostumeName}");
        }

        public static void ClearCostumeData()
        {
            _costumeDataList.Clear();
            Debug.Log("Clear CostumeData");
        }

        public static void SetCharacterSettingIndex(int characterSettingIndex)
        {
            _characterSettingIndex = characterSettingIndex;
        }
        
        public static PlayerSkin InstantiateCharacter(Transform parent)
        {
            if(!_characterSettingModel || _characterSettingIndex < 0 || _characterSettingIndex > _characterSettingModel.AllPlayerData.Count)
            {
                return null;
            }

            PlayerSkin character = Instantiate(_characterSettingModel.AllPlayerData[_characterSettingIndex].PlayerSkin, parent);
            LoadCharacterCostume(_characterSettingModel.AllPlayerData[_characterSettingIndex], character);
            return character;
        }

        private static void LoadCharacterCostume(PlayerData playerData, PlayerSkin character)
        {
            _curPlayerData = playerData;
            for (int i = 0; i < _costumeDataList.Count; i++)
            {
                CostumeData costumeData = _costumeDataList[i];  
                Transform costumePoint = GetCostumePoint(costumeData, character);
                if (costumePoint != null)
                {
                    Instantiate(costumeData.CostumePrefab, costumePoint);
                }
            }
        }

        public static Transform GetCostumePoint(CostumeData costumeData, PlayerSkin playerSkin)
        {
            Transform costumePoint = costumeData.CostumeCategory switch
            {
                CostumeCategory.Cap => playerSkin.CapPoint,
                CostumeCategory.Glasses => playerSkin.GlassesPoint,
                CostumeCategory.Cape => playerSkin.CapePoint,
                _ => null,
            };

            return costumePoint;
        }
        
        public static PlayerData GetCurrentPlayerData()
        {
            return _curPlayerData;
        }
    }
}
