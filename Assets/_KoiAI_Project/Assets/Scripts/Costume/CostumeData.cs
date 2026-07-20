
using UnityEngine;

namespace KoiAI.Costume
{
    [CreateAssetMenu(fileName = "new CostumeData", menuName = "KoiAI/Costume/CostumeData")]
    public class CostumeData : ScriptableObject
    {
        [SerializeField]
        private CustomeCategory _costumeCategory;
        [SerializeField]
        private string _costumeName;
        [SerializeField]
        private GameObject _costumePrefab;

        public CustomeCategory CostumeCategory => _costumeCategory;
        public string CostumeName => _costumeName;
        public GameObject CostumePrefab => _costumePrefab;  
    }
}
