using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.UI;

    //런타임 용 종이 아이템
    public class PaperItem : ResourceBase
    {
        [SerializeField]
        private PaperData _paperData;

        public override ItemData GetItemData() => InitializedItemData ?? _paperData;

        public override void UseItem()
        {
            if(ItemOwner == null)
            {
                return;
            }

            PaperData paperData = GetItemData() as PaperData;
            if (paperData == null)
            {
                return;
            }

            PaperPopupWindow.Open(paperData);
        }
    }
}
