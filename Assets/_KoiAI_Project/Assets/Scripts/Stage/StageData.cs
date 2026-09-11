using UnityEngine;

namespace KoiAI.Stage
{
    [CreateAssetMenu(fileName = "new StageData", menuName = "KoiAI/StageData")]
    public class StageData : ScriptableObject
    {
        [SerializeField]
        private string _stageName;

        public string StageName => _stageName;
    }
}
