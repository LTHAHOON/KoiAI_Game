using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KoiAI.Utilities
{
    public class PinCodeInputField : TMP_InputField
    {
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            base.OnUpdateSelected(eventData);

            if (isFocused)
            {
                MoveTextEnd(false);
            }
        }
    }
}
