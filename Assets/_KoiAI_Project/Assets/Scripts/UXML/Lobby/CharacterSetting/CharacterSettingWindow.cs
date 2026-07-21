
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    using KoiAI.Costume;
    using KoiAI.Player;
    using KoiAI.Utilities;

    public class CharacterSettingWindow : PopUpWindow_UID<CharacterSettingModel, CharacterSettingViewInfo>, ICircleColorPickerHandler
    {
        [SerializeField]
        private CinemachinePositionComposer _cmPositionComposer;
        [SerializeField]
        private Transform _previewCharacterParent;
        [SerializeField]
        private float _activePointOffsetY = 10;

        private Vector3 _activePoint = Vector3.zero;
        private Button _confirmButton;
        private Button _cancelButton;

        private Label _characterScrollerName;
        private RepeatButton _charChangeLeftButton;
        private RepeatButton _charChangeRightButton;
        private Dictionary<int, PlayerSkin> _previewCharacters;

        private CircleColorPicker _circleColorPicker_Face;
        private CircleColorPicker _circleColorPicker_Body;

        private List<RadioButton> _costumeSearchRadioButtons;
        private RadioButtonGroup _costumeSearchRadioGroup;
        private CostumeCategory _curSearchCostumeCategory = CostumeCategory.Cap;
        private readonly int _initialCostumeSearchIndex = 0;

        private int _currentCharIndex = 0;
        private int _lastCharIndex = 0;
        private bool _isConfirmed = false;

        public override void Initalize(CharacterSettingModel visualModel, CharacterSettingViewInfo viewInfo)
        {
            PopUpWindowContainer<UIDocument, string> windowContainer = GetPopUpWindowContainer();
            if (windowContainer == null)
            {
                return;
            }

            #region 프리뷰 캐릭터 생성
            _previewCharacters = new Dictionary<int, PlayerSkin>();
            for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
            {
                PlayerSkin playerSkin = Instantiate(visualModel.AllPlayerData[i].PlayerSkin, _previewCharacterParent);
                playerSkin.gameObject.SetActive(false);
                _previewCharacters.Add(i, playerSkin);
            }
            #endregion

            #region Confirm && Cancel 버튼 초기화
            VisualElement characterSettingRoot = windowContainer.Parent.rootVisualElement.Q<VisualElement>(windowContainer.Window);
            _confirmButton = characterSettingRoot.Q<Button>(viewInfo.ConfirmBtnName);
            _cancelButton = characterSettingRoot.Q<Button>(viewInfo.CancelBtnName);
            _confirmButton.clicked += OnClickConfirmButton;
            _cancelButton.clicked += OnClickCancelButton;
            #endregion

            #region 캐릭터 변경 버튼 초기화
            VisualElement characterScroller = characterSettingRoot.Q<VisualElement>(viewInfo.CharacterScrollerName);
            _charChangeLeftButton = characterScroller.Q<RepeatButton>("unity-low-button");
            _charChangeRightButton = characterScroller.Q<RepeatButton>("unity-high-button");

            _characterScrollerName = characterScroller.Q<Label>(viewInfo.CharacterScrollerLabelName);
            _characterScrollerName.dataSource = visualModel;
            _characterScrollerName.SetBinding("text", new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(visualModel.CurrentPlayerName)),
                bindingMode = BindingMode.ToTarget
            });

            _charChangeLeftButton.SetAction(() => OnChangeCharacter(-1, visualModel), visualModel.CharChangeRpeatBtnDelay, visualModel.CharChangeRpeatBtnInterval);
            _charChangeRightButton.SetAction(() => OnChangeCharacter(1, visualModel), visualModel.CharChangeRpeatBtnDelay, visualModel.CharChangeRpeatBtnInterval);
            OnChangeCharacter(0, visualModel);
            #endregion

            #region CircleColorPicker 초기화
            VisualElement circleColorPicker_Face_Root = windowContainer.Parent.rootVisualElement.Q<VisualElement>(viewInfo.CircleColorPickerName_Face);
            VisualElement circleColorPicker_Body_Root = windowContainer.Parent.rootVisualElement.Q<VisualElement>(viewInfo.CircleColorPickerName_Body);
            _circleColorPicker_Face = new CircleColorPicker(this, circleColorPicker_Face_Root, viewInfo.CirclePaletteName, viewInfo.PalettePickerName);
            _circleColorPicker_Body = new CircleColorPicker(this, circleColorPicker_Body_Root, viewInfo.CirclePaletteName, viewInfo.PalettePickerName);

            Button palettePcikerCenterBtn_Face =  circleColorPicker_Face_Root.Q<Button>(viewInfo.PalettePickerCenterBtnName);
            Button palettePcikerCenterBtn_Body = circleColorPicker_Body_Root.Q<Button>(viewInfo.PalettePickerCenterBtnName);
            palettePcikerCenterBtn_Face.clicked += _circleColorPicker_Face.MovePickerToCenter;
            palettePcikerCenterBtn_Body.clicked += _circleColorPicker_Body.MovePickerToCenter;
            #endregion

            #region CostumeSlotList 초기화
            VisualElement costumeSlotList = windowContainer.Parent.rootVisualElement.Q<VisualElement>(viewInfo.CostumeSlotList);
            ScrollView costumeScrollView = costumeSlotList.Q<ScrollView>(viewInfo.CostumeScrollView);
            for (int i = 0; i < visualModel.CostumeDataBases.Length; i++)
            {
                List<CostumeData> costumeDataList =  visualModel.CostumeDataBases[i].GetCostumeDataList();

                for (int j = 0; j < costumeDataList.Count; j++)
                {

                    CostumeData costumeData = costumeDataList[j];

                    VisualElement newCostumeSlot = viewInfo.CostumeSlotTemplate.CloneTree();
                    newCostumeSlot.AddToClassList("CostumeSlot");

                    Label costumeName = newCostumeSlot.Q<Label>(viewInfo.CostumeTextName);
                    costumeName.text = costumeData.CostumeName;
                    Image costumeImage = newCostumeSlot.Q<Image>(viewInfo.CostumeImageName);
                    costumeImage.image = costumeData.CostumeTexture;

                    costumeData.SetCostumeSlot(newCostumeSlot);
                    costumeScrollView.Add(newCostumeSlot);
                    newCostumeSlot.style.display = DisplayStyle.None;

                    Button costumeEquipButton = newCostumeSlot.Q<Button>(viewInfo.CostumeEquipBtnName);
                    costumeEquipButton.clicked += () => EquipCostume(costumeData);
                }
            }

            _costumeSearchRadioGroup = costumeSlotList.Q<RadioButtonGroup>(viewInfo.CostumeSearchRadioGroupName);
            _costumeSearchRadioButtons = _costumeSearchRadioGroup.Query<RadioButton>().ToList();

            _costumeSearchRadioGroup.RegisterValueChangedCallback(evt =>
            {
                if(!Enum.IsDefined(typeof(CostumeCategory), evt.newValue))
                {
                    return;
                }
                CostumeCategory searchCategory = (CostumeCategory)evt.newValue;
                SearchCostume(searchCategory);
            });

            #endregion
        }

        public void EquipCostume(CostumeData costumeData)
        {

        }

        public void SearchCostume(CostumeCategory searchCostumeCategory)
        {
            if(_costumeSearchRadioGroup == null)
            {
                return;
            }

            CharacterSettingModel visualModel = GetVisualModel();

            bool bCurGet = visualModel.TryGetCostumeDataBase(out CostumeDataBase curCostumeDataBase, _curSearchCostumeCategory);
            bool bOtherGet = visualModel.TryGetCostumeDataBase(out CostumeDataBase targetCostumeDataBase, searchCostumeCategory);

            if (bCurGet && bOtherGet)
            {
                List<CostumeData> costumeDataList = curCostumeDataBase.GetCostumeDataList();
                SetDisplayCostumeSlot(DisplayStyle.None, costumeDataList);

                costumeDataList = targetCostumeDataBase.GetCostumeDataList();
                SetDisplayCostumeSlot(DisplayStyle.Flex, costumeDataList);

                _curSearchCostumeCategory = searchCostumeCategory;
            }
        }

        private void SetDisplayCostumeSlot(DisplayStyle displayStyle, List<CostumeData> costumeDataList)
        {
            for (int i = 0; i < costumeDataList.Count; i++)
            {
                costumeDataList[i].CostumeSlot.style.display = displayStyle;
            }
        }

        //PopUpWindow 콜백으로 호출됩니다.
        public override void OnCompleteOpen()
        {
            if(_circleColorPicker_Face == null || _circleColorPicker_Body == null)
            {
                return;
            }

            CharacterSettingModel visualModel =  GetVisualModel();
            if (visualModel.AllPlayerData.Count > _currentCharIndex)
            {
                _circleColorPicker_Face.RegisterAllCallBack(visualModel.AllPlayerData[_currentCharIndex].CurColorPosition_Face);
                _circleColorPicker_Body.RegisterAllCallBack(visualModel.AllPlayerData[_currentCharIndex].CurColorPosition_Body);
            }
            for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
            {
                visualModel.AllPlayerData[i].SetLastColorPosition();
            }
            _lastCharIndex = _currentCharIndex;

            _costumeSearchRadioButtons[_initialCostumeSearchIndex].value = true;

            SetConfirmButtonEnabled(false);
        }

        // TODO: 버튼 방향에 맞게 캐릭터 바꾸기
        private async void OnChangeCharacter(int direction, CharacterSettingModel visualModel)
        {
            SetActivePreviewCharacter(_currentCharIndex, false);

            int AllCharacterDataCount = visualModel.AllPlayerData.Count;
            int index = (_currentCharIndex + direction + AllCharacterDataCount) % AllCharacterDataCount;
            _currentCharIndex = index;

            SetActivePreviewCharacter(_currentCharIndex, true);
            
            await RefreshPreviewCameraPos(_currentCharIndex, visualModel);
            SetConfirmButtonEnabled(true);

            PlayerData curPlayerData = visualModel.AllPlayerData[_currentCharIndex];
            visualModel.CurrentPlayerName = curPlayerData.CharacterBaseName;
            //캐릭터 바뀔 때만 해당 캐릭터의 Color 위치 데이터 불러오기 
            if (direction != 0)
            {
                _circleColorPicker_Face.RegisterAllCallBack(curPlayerData.CurColorPosition_Face);
                _circleColorPicker_Body.RegisterAllCallBack(curPlayerData.CurColorPosition_Body);    
            }
        }

        private async void UndoToChangeCharacter(CharacterSettingModel visualModel)
        {
            SetActivePreviewCharacter(_currentCharIndex, false);
            _currentCharIndex = _lastCharIndex;

            SetActivePreviewCharacter(_currentCharIndex, true);
            await RefreshPreviewCameraPos(_currentCharIndex, visualModel);
            
            visualModel.CurrentPlayerName = visualModel.AllPlayerData[_currentCharIndex].CharacterBaseName;
        }

        /// <summary>
        /// 캐릭터를 보는 카메라 위치 설정하기
        /// </summary>
        private async UniTask RefreshPreviewCameraPos(int index, CharacterSettingModel visualModel)
        {
            if (visualModel.AllPlayerData.Count <= index)
            {
                return;
            }

            PlayerData playerData = visualModel.AllPlayerData[index];
            if (playerData)
            {
                switch (playerData.PlayerFeatureDataType)
                {
                    case PlayerFeatureDataType.Small:
                        _cmPositionComposer.TargetOffset.x = 0f;
                        _cmPositionComposer.TargetOffset.z = 0f;
                        break;
                    case PlayerFeatureDataType.Medium:
                        _cmPositionComposer.TargetOffset.x = 0f;
                        _cmPositionComposer.TargetOffset.z = 0f;
                        break;
                    case PlayerFeatureDataType.Large:
                        _cmPositionComposer.TargetOffset.x = 2.5f;
                        _cmPositionComposer.TargetOffset.z = 5f;
                        break;
                }
            }

            if(index <= 0)
            {
                _cmPositionComposer.VirtualCamera.PreviousStateIsValid = false;
                _cmPositionComposer.Damping.y = 0f;
                _cmPositionComposer.TargetOffset.y = 0f;

                //한 프레임 시네머신에 넘겨주기
                await UniTask.NextFrame();
            }
            _cmPositionComposer.Damping.y = 1f;
            _cmPositionComposer.TargetOffset.y += _activePointOffsetY;
        }

        /// <summary>
        /// 캐릭터 프리뷰 Active 설정하기
        /// </summary>
        private void SetActivePreviewCharacter(int index, bool active)
        {
            if (_previewCharacters.TryGetValue(index, out PlayerSkin playerSkin))
            {
                if (active)
                {
                    if (index <= 0)
                    {
                        _activePoint.y = _activePointOffsetY;
                    }
                    else
                    {
                        _activePoint.y += _activePointOffsetY;
                    }
                    playerSkin.transform.position = _activePoint;
                }
                playerSkin.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 적용 버튼 Enabled 설정하기
        /// </summary>
        private void SetConfirmButtonEnabled(bool enabled)
        {
            _confirmButton.enabledSelf = enabled;
            _isConfirmed = !enabled;
        }

        //적용 버튼 누를 때 호출됩니다.
        private void OnClickConfirmButton()
        {
            SetConfirmButtonEnabled(false);
            CharacterSettingModel visualModel = GetVisualModel();
            for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
            {
                visualModel.AllPlayerData[i].SetLastColorPosition();
            }
            _lastCharIndex = _currentCharIndex;
        }

        //닫기 버튼 누를 때 호출됩니다.
        private void OnClickCancelButton()
        {
            CharacterSettingModel visualModel = GetVisualModel();

            //적용 버튼을 누르지 않을 경우 이전 색으로 되돌리기
            if (!_isConfirmed)
            {
                UndoToChangeCharacter(visualModel);

                for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
                {
                    PlayerData playerData = visualModel.AllPlayerData[i];
                    if (!playerData)
                    {
                        continue;
                    }

                    playerData.UndoToLastColorPosition();
                    if(i == _lastCharIndex)
                    {
                        _circleColorPicker_Face.UnregisterAllCallBack(playerData.CurColorPosition_Face);
                        _circleColorPicker_Body.UnregisterAllCallBack(playerData.CurColorPosition_Body);
                    }
                }
            }

            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        /// <summary>
        /// ICircleColorPickerHandler로 인해 컬러가 결정될 때 호출됩니다.
        /// </summary>
        public void OnColorChanged(CircleColorPicker circleColorPicker, Color newColor, Vector2 curColorPosition)
        {
            CharacterSettingModel visualModel =  GetVisualModel();
            PlayerData curPlayerData = visualModel.AllPlayerData[_currentCharIndex];
            PlayerSkin curPlayerSkin = curPlayerData.PlayerSkin;
    
            //material 객체를 생성하지않고 직접 material color설정
            if (circleColorPicker == _circleColorPicker_Face)
            {
                curPlayerSkin.FaceMaterial.color = newColor;
                
                curPlayerData.SetCurColorPositionFace(curColorPosition);
            }
            else if (circleColorPicker == _circleColorPicker_Body)
            {
                curPlayerSkin.BodyMaterial.color = newColor;
                curPlayerData.SetCurColorPositionBody(curColorPosition);
            }

            SetConfirmButtonEnabled(true);
        }
    }
}
