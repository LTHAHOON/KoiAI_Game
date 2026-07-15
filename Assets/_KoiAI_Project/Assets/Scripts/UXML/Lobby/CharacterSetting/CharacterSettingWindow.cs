
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class CharacterSettingWindow : PopUpWindow_UID<CharacterSettingModel, CharacterSettingViewInfo>
    {
        private Button _confirmButton;
        private Button _cancelButton;
        private RepeatButton _charChangeLeftButton;
        private RepeatButton _charChangeRightButton;
        private Label _characterScrollerName;

        public override void Initalize(CharacterSettingModel visualModel, CharacterSettingViewInfo viewInfo)
        {
            PopUpWindowContainer<UIDocument, string> windowContainer = GetPopUpWindowContainer();
            if (windowContainer == null)
            {
                return;
            }

            VisualElement characterSettingRoot = windowContainer.Parent.rootVisualElement.Q<VisualElement>(windowContainer.Window);
            _confirmButton = characterSettingRoot.Q<Button>(viewInfo.ConfirmBtnName);
            _cancelButton = characterSettingRoot.Q<Button>(viewInfo.CancelBtnName);

            _confirmButton.clicked += OnClickConfirmButton;
            _cancelButton.clicked += OnClickCancelButton;

            Scroller characterScroller = characterSettingRoot.Q<Scroller>(viewInfo.CharacterScrollerName);
            _charChangeLeftButton = characterScroller.Q<RepeatButton>("unity-low-button");
            _charChangeRightButton = characterScroller.Q<RepeatButton>("unity-high-button");

            _characterScrollerName = characterScroller.Q<Label>(viewInfo.CharacterScrollerName);
            
            _characterScrollerName.bindingPath = nameof(visualModel.CurrentPlayerData.CharacterBaseName);
            _characterScrollerName.dataSource = visualModel.CurrentPlayerData;

           // _charChangeLeftButton. += ()=> OnChangeCharacter(-1, visualModel);
           // _charChangeRightButton.clicked += () => OnChangeCharacter(1, visualModel);

        }

        private void OnChangeCharacter(int direction, CharacterSettingModel visualModel)
        {
            int AllCharacterDataCount = visualModel.AllPlayerData.Length;
            int index = (direction + AllCharacterDataCount) % AllCharacterDataCount;
            _characterScrollerName.dataSource = visualModel.AllPlayerData[index];
        }

        private void OnClickConfirmButton()
        {
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        private void OnClickCancelButton()
        {
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }
    }
}
