using UnityEngine;

namespace KoiAI.Item
{
    //종이 데이터 저장용
    [CreateAssetMenu(fileName = "new PaperData", menuName = "KoiAI/Item/PaperData")]
    public class PaperData : ItemData
    {
        [SerializeField]
        private Texture2D _paperTex;

        public Texture2D PaperTex => _paperTex;
    }
}
