using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace KoiAI.UI
{
    using KoiAI.Costume;
    using KoiAI.Player;
    using NaughtyAttributes;
    using System;

    [CreateAssetMenu(fileName = "new CharacterSettingModel", menuName = "KoiAI/UI/Model/CharacterSettingModel")]
    public class CharacterSettingModel : VisualModel
    {
        [SerializeField]
        private List<PlayerData> _allPlayerData;
        [SerializeField]
        private long _charChangeRepeatBtnDelay;
        [SerializeField]
        private long _charChangeRepeatBtnInterval;
        [SerializeField]
        private CostumeDataBase[] _costumeDataBases;

        [CreateProperty]
        public string CurrentPlayerName { get; set; }

        public List<PlayerData> AllPlayerData => _allPlayerData;
        public long CharChangeRpeatBtnDelay => _charChangeRepeatBtnDelay;
        public long CharChangeRpeatBtnInterval => _charChangeRepeatBtnInterval;
        public CostumeDataBase[] CostumeDataBases => _costumeDataBases;

        [Button("Clear All CostumeGUIDs of Player")]
        public void ClearAllCostumeGUIDs()
        {
            List<PlayerData> playerDataList = _allPlayerData;
            for (int i = 0; i < playerDataList.Count; i++)
            {
                List<Guid> guids = playerDataList[i].GetWearingCostumeGUIDs();
                guids.Clear();
            }
            Debug.Log($"Complete: Clear Costume GUIDs of {playerDataList.Count} Player");
        }

        public bool TryGetCostumeDataBase(out CostumeDataBase costumeDataBase, CostumeCategory costumeCategory)
        {
            for (int i = 0; i < _costumeDataBases.Length; i++)
            {
                if (_costumeDataBases[i].CostumeCategory == costumeCategory)
                {
                    costumeDataBase = _costumeDataBases[i];
                    return true;
                }
            }
            costumeDataBase = default;
            return false;
        }

    }
}
