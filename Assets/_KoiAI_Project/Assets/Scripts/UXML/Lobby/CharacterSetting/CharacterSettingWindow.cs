
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        private Dictionary<int, PlayerSkin> _dicPreviewCharacters;

        private CircleColorPicker _circleColorPicker_Face;
        private CircleColorPicker _circleColorPicker_Body;

        private struct CostumeSlotData
        {
            #region CostumeSlotData
            private CostumeData _costumeData;
            private Button _costumeWearButton;
            private Button _costumeWearingButton;

            public CostumeSlotData(CostumeData costumeData, Button costumeWearButton, Button costumeWeaingButton)
            {
                _costumeData = costumeData;
                _costumeWearButton = costumeWearButton;
                _costumeWearingButton = costumeWeaingButton;
            }

            public CostumeData CostumeData => _costumeData;
            public Button CostumeWearButton => _costumeWearButton;
            public Button CostumeWearingButton => _costumeWearingButton;
            #endregion
        }
        private Dictionary<Guid, GameObject> _dicWearingCostumes;
        private Dictionary<Guid, CostumeSlotData> _dicWearingCostumeSlotData;
        private List<RadioButton> _costumeSearchRadioButtons;
        private RadioButtonGroup _costumeSearchRadioGroup;
        private CostumeCategory _curSearchCostumeCategory = CostumeCategory.Cap;
        private Action<List<Guid>> OnLoadWearingCostume;
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
            _dicPreviewCharacters = new Dictionary<int, PlayerSkin>();
            for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
            {
                PlayerSkin playerSkin = Instantiate(visualModel.AllPlayerData[i].PlayerSkin, _previewCharacterParent);
                playerSkin.gameObject.SetActive(false);
                _dicPreviewCharacters.Add(i, playerSkin);
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
            _dicWearingCostumes = new();
            _dicWearingCostumeSlotData = new();
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

                    Button costumeWearButton = newCostumeSlot.Q<Button>(viewInfo.CostumeWearBtnName);
                    Button costumeWearingButton = newCostumeSlot.Q<Button>(viewInfo.CostumeWearingBtnName);

                    //착용 중 버튼 클릭시 TakeOffCostume 호출
                    costumeWearingButton.clicked += () => TakeOffCostume(costumeData, costumeWearButton, costumeWearingButton);
                    //착용 버튼 클릭시 WearCostume 호출
                    costumeWearButton.clicked += () =>
                    {
                        //중복이 허용되지 않는 코스튬일 경우 해당 카테고리 전부 착용 해제
                        if (!costumeData.CanMulitpleCostume)
                        {
                            TakeOffCostume(costumeData.CostumeCategory);
                        }
                        WearCostume(costumeData, costumeWearButton, costumeWearingButton);
                    };

                    //코스튬 불러오기 및 버튼 새로고침 등록
                    OnLoadWearingCostume += (guidList) =>
                    {
                        Guid guidToCompare = costumeData.GetGUID();
                        if (guidList.Contains(guidToCompare))
                        {
                            WearCostume(costumeData, costumeWearButton, costumeWearingButton);
                        }
                        else
                        {
                            TakeOffCostume(costumeData, costumeWearButton, costumeWearingButton);
                        }
                    };
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
            CharacterSettingService.Init(visualModel, _currentCharIndex);
            LoadWearingCostume(visualModel.AllPlayerData[_currentCharIndex]);
            #endregion

        }

        private void LoadWearingCostume(PlayerData playerData)
        {
            foreach(GameObject costume in _dicWearingCostumes.Values)
            {
                Destroy(costume);
            }
            _dicWearingCostumes.Clear();
            _dicWearingCostumeSlotData.Clear();
            CharacterSettingService.ClearCostumeData();

            List<Guid> weaingCostumeGuids =  playerData.GetWearingCostumeGUIDs();
            OnLoadWearingCostume?.Invoke(weaingCostumeGuids);

        }


        private void UndoWearingCostume()
        {
            CharacterSettingModel visualModel = GetVisualModel();
            List<PlayerData> playerDataList = visualModel.AllPlayerData;
            List<Guid> lastWearingCostumeGuids;
            for (int i = 0; i < playerDataList.Count; i++)
            {
                PlayerData playerData = playerDataList[i];
                List<Guid> wearingCostumeGuids =  playerData.GetWearingCostumeGUIDs();
                lastWearingCostumeGuids = playerDataList[i].GetLastWearingCostumeGUIDs();

                wearingCostumeGuids.Clear();
                foreach(Guid guid in lastWearingCostumeGuids)
                {
                    if(wearingCostumeGuids.Contains(guid))
                    {
                        wearingCostumeGuids.Remove(guid);
                    }
                    else
                    {
                        wearingCostumeGuids.Add(guid);
                    }
                }
                if(i == _currentCharIndex)
                {
                    OnLoadWearingCostume?.Invoke(lastWearingCostumeGuids);
                }
                lastWearingCostumeGuids.Clear();
            }
        }

        private void WearCostume(CostumeData costumeData, Button wearButton, Button wearingButton)
        {
            Guid guid = costumeData.GetGUID();
            //중복 생성 방지
            if (_dicWearingCostumes.ContainsKey(guid) || _dicWearingCostumeSlotData.ContainsKey(guid))
            {
                return;
            }
            if (_dicPreviewCharacters.TryGetValue(_currentCharIndex, out PlayerSkin playerSkin))
            {
                Transform costumePoint = CharacterSettingService.GetCostumePoint(costumeData, playerSkin);
                if (costumePoint != null)
                {
                    GameObject costume = Instantiate(costumeData.CostumePrefab, costumePoint);
                    costume.transform.localPosition = Vector3.zero;


                    //Costume ID 저장
                    List<Guid> wearingCostumeGuids = GetPlayerWearingCostumeGuids(_currentCharIndex);
                    if (!wearingCostumeGuids.Contains(guid))
                    {
                        wearingCostumeGuids.Add(guid);
                    }
                    CharacterSettingService.AddCostumeData(costumeData);

                    wearButton.style.display = DisplayStyle.None;
                    wearingButton.style.display = DisplayStyle.Flex;
                    _dicWearingCostumes.Add(guid, costume);
                    _dicWearingCostumeSlotData.Add(guid, new(costumeData, wearButton, wearingButton));

                    SetConfirmButtonEnabled(true);
                }
            }
        }

        private readonly List<Guid> _wearingCostumeKeyToRemove = new();
        private bool _bTakingOffUsingCategory = false;
        private void TakeOffCostume(CostumeCategory costumeCategory)
        {
            _bTakingOffUsingCategory = true;
            foreach (Guid guid in _dicWearingCostumeSlotData.Keys)
            {
                CostumeSlotData costumeSlotData = _dicWearingCostumeSlotData[guid];
                if (costumeSlotData.CostumeData.CostumeCategory == costumeCategory)
                {
                    TakeOffCostume(costumeSlotData.CostumeData, costumeSlotData.CostumeWearButton, costumeSlotData.CostumeWearingButton);
                    _wearingCostumeKeyToRemove.Add(guid);
                }
            }
            foreach(Guid guid in _wearingCostumeKeyToRemove)
            {
                _dicWearingCostumeSlotData.Remove(guid);
                Debug.Log(_dicWearingCostumeSlotData.Count);
            }
            _wearingCostumeKeyToRemove.Clear();
            _bTakingOffUsingCategory = false;
        }

        private void TakeOffCostume(CostumeData costumeData, Button wearButton, Button wearingButton)
        {
            Guid guid = costumeData.GetGUID();
            if (_dicWearingCostumes.TryGetValue(guid, out GameObject wearingCostume))
            {
                Destroy(wearingCostume);

                _dicWearingCostumes.Remove(guid);
                if(!_bTakingOffUsingCategory)
                {
                    _dicWearingCostumeSlotData.Remove(guid);
                }
                CharacterSettingService.RemoveCostumeData(costumeData);
            }

            //Costume ID 제거
            List<Guid> wearingCostumeGuids = GetPlayerWearingCostumeGuids(_currentCharIndex);
            wearingCostumeGuids.Remove(guid);

            
            wearingButton.style.display = DisplayStyle.None;
            wearButton.style.display = DisplayStyle.Flex;
        
            SetConfirmButtonEnabled(true);
        }

        public List<Guid> GetPlayerWearingCostumeGuids(int index)
        {
            CharacterSettingModel visualModel = GetVisualModel();
            PlayerData curPlayerData = visualModel.AllPlayerData[index];
            List<Guid> wearingCostumeGuids = curPlayerData.GetWearingCostumeGUIDs();
            return wearingCostumeGuids;
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
            PlayerData curPlayerData = visualModel.AllPlayerData[_currentCharIndex];
            if (visualModel.AllPlayerData.Count > _currentCharIndex)
            {
                _circleColorPicker_Face.RegisterAllCallBack(curPlayerData.CurColorPosition_Face);
                _circleColorPicker_Body.RegisterAllCallBack(curPlayerData.CurColorPosition_Body);
            }
            for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
            {
                visualModel.AllPlayerData[i].SetLastColorPosition();
                visualModel.AllPlayerData[i].SaveWearingCostumeGUIDs();

            }
            _lastCharIndex = _currentCharIndex;

            _costumeSearchRadioButtons[_initialCostumeSearchIndex].value = true;

           
            LoadWearingCostume(curPlayerData);
            SetConfirmButtonEnabled(false);
        }

        //PopUpWindow 콜백으로 호출됩니다.
        public override void OnCompleteClose()
        {
            CharacterSettingModel visualModel = GetVisualModel();

            //적용 버튼을 누르지 않을 경우 이전 데이터로 되돌리기
            if (!_isConfirmed)
            {
                UndoToChangeCharacter(visualModel);
                UndoWearingCostume();

                for (int i = 0; i < visualModel.AllPlayerData.Count; i++)
                {
                    PlayerData playerData = visualModel.AllPlayerData[i];
                    if (!playerData)
                    {
                        continue;
                    }

                    playerData.UndoToLastColorPosition();
                    if (i == _lastCharIndex)
                    {
                        _circleColorPicker_Face.UnregisterAllCallBack(playerData.CurColorPosition_Face);
                        _circleColorPicker_Body.UnregisterAllCallBack(playerData.CurColorPosition_Body);
                    }
                }
            }
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
            //캐릭터 바뀔 때만 해당 캐릭터의 데이터 불러오기 
            if (direction != 0)
            {
                _circleColorPicker_Face.RegisterAllCallBack(curPlayerData.CurColorPosition_Face);
                _circleColorPicker_Body.RegisterAllCallBack(curPlayerData.CurColorPosition_Body);
                LoadWearingCostume(curPlayerData);
                CharacterSettingService.SetCharacterSettingIndex(_currentCharIndex);
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
            if (_dicPreviewCharacters.TryGetValue(index, out PlayerSkin playerSkin))
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
                visualModel.AllPlayerData[i].SaveWearingCostumeGUIDs();

            }
            _lastCharIndex = _currentCharIndex;

        }

        //닫기 버튼 누를 때 호출됩니다.
        private void OnClickCancelButton()
        {
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
