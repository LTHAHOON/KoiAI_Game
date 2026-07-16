using KoiAI.Player;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace KoiAI
{
    [CreateAssetMenu(fileName = "new CharacterSettingModel", menuName = "KoiAI/UI/Model/CharacterSettingModel")]
    public class CharacterSettingModel : VisualModel
    {
        [SerializeField]
        private List<PlayerData> _allPlayerData;
        [SerializeField]
        private long _charChangeRepeatBtnDelay;
        [SerializeField]
        private long _charChangeRepeatBtnInterval;

        [CreateProperty]
        public string CurrentPlayerName { get; set; }

        public List<PlayerData> AllPlayerData => _allPlayerData;
        public long CharChangeRpeatBtnDelay => _charChangeRepeatBtnDelay;
        public long CharChangeRpeatBtnInterval => _charChangeRepeatBtnInterval;

    }
}
