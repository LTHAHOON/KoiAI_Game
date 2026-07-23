using UnityEngine;

namespace KoiAI
{
    using KoiAI.Player;
    using KoiAI.UI;

    public class CharacterSettingService : MonoBehaviour
    {
        private static CharacterSettingModel _characterSettingModel;
        private static int _characterSettingIndex;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void SetPlayerCharacter(CharacterSettingModel characterSettingModel, int characterSettingIndex)
        {
            _characterSettingModel = characterSettingModel;
            _characterSettingIndex = characterSettingIndex;
        }

        public static PlayerSkin InstantiateCharacter(Transform parent)
        {
            if(!_characterSettingModel || _characterSettingIndex < 0 || _characterSettingIndex > _characterSettingModel.AllPlayerData.Count)
            {
                return null;
            }

            PlayerSkin character = Instantiate(_characterSettingModel.AllPlayerData[_characterSettingIndex].PlayerSkin, parent);
            LoadCharacterCostume(character);
            return character;
        }

        private static void LoadCharacterCostume(PlayerSkin playerSkin)
        {

        }
    }
}
