using KoiAI.Player;
using UnityEngine;

namespace KoiAI
{
    [CreateAssetMenu(fileName = "new CharacterSettingModel", menuName = "KoiAI/UI/Model/CharacterSettingModel")]
    public class CharacterSettingModel : VisualModel
    {
        [SerializeField]
        private PlayerData[] _allPlayerData;

        [SerializeField]
        private PlayerData _currentPlayerData;

        public PlayerData[] AllPlayerData => _allPlayerData;
        public PlayerData CurrentPlayerData => _currentPlayerData;
    }
}
