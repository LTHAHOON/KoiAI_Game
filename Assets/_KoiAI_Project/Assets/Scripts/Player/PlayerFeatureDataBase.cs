using KoiAI.Player;
using UnityEngine;

namespace KoiAI.Player
{
    [CreateAssetMenu(fileName = "new PlayerFeatureDataBase", menuName = "KoiAI/Player/PlayerFeatureDataBase")]
    public class PlayerFeatureDataBase : ScriptableObject
    {
        [SerializeField]
        private PlayerFeatureData[] _playerFeatureDatas;
            
        public PlayerFeatureData GetPlayerFeatureData(PlayerFeatureDataType playerFeatureDataType)
        {
            for (int i = 0; i < _playerFeatureDatas.Length; i++)
            {
                PlayerFeatureData playerFeatureData = _playerFeatureDatas[i];
                if (playerFeatureData.PlayerFeatureDataType == playerFeatureDataType)
                {
                    return playerFeatureData;
                }
            }

            return null;
        }
    }
}
