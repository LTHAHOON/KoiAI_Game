using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace KoiAI.UI
{
    using KoiAI.Costume;
    using KoiAI.Player;

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
