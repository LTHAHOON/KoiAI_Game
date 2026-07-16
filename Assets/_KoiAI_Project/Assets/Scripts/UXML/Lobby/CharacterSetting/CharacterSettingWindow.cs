
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;

namespace KoiAI.UI
{
    using KoiAI.Utilities;
    using KoiAI.Player;

    public class CharacterSettingWindow : PopUpWindow_UID<CharacterSettingModel, CharacterSettingViewInfo>, ICircleColorPickerHandler
    {
        [SerializeField]
        private CinemachinePositionComposer _cmPositionComposer;
        [SerializeField]
        private Transform _previewCharacterParent;
        [SerializeField]
        private float _activePointOffsetY = 10;

        private Vector3 _activePoint = Vector3.zero;
        private Dictionary<int, PlayerSkin> _previewCharacters;
        private Button _confirmButton;
        private Button _cancelButton;
        private RepeatButton _charChangeLeftButton;
        private RepeatButton _charChangeRightButton;
        private Label _characterScrollerName;
        private CircleColorPicker _circleColorPicker;
        private int _currentCharIndex = 0;

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

            _circleColorPicker = new CircleColorPicker(this, characterSettingRoot, viewInfo.CirclePaletteName, viewInfo.PalettePickerName);
        }

        //PopUpWindow 콜백으로 호출됩니다.
        public override void OnCompleteOpen()
        {
            if(_circleColorPicker == null)
            {
                return;
            }
            _circleColorPicker.RegisterAllCallBack();
        }

        // TODO: 버튼 방향에 맞게 캐릭터 바꾸기
        private async void OnChangeCharacter(int direction, CharacterSettingModel visualModel)
        {
            SetActivePreviewCharacter(_currentCharIndex, false);

            int AllCharacterDataCount = visualModel.AllPlayerData.Count;
            int index = (_currentCharIndex + direction + AllCharacterDataCount) % AllCharacterDataCount;
            visualModel.CurrentPlayerName = visualModel.AllPlayerData[index].CharacterBaseName;
            _currentCharIndex = index;


            SetActivePreviewCharacter(_currentCharIndex, true);
            
            await RefreshPreviewCameraPos(_currentCharIndex, visualModel);
            SetConfirmButtonEnabled(true);
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
        }

        //적용 버튼 누를 때
        private void OnClickConfirmButton()
        {
            SetConfirmButtonEnabled(false);
        }

        //닫기 버튼 누를 때
        private void OnClickCancelButton()
        {
            _circleColorPicker.UnregisterAllCallBack();
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        /// <summary>
        /// ICircleColorPickerHandler로 인해 컬러가 결정될 때 호출됩니다.
        /// </summary>
        public void OnColorChanged(Color newColor)
        {
            CharacterSettingModel model = GetModel();
            PlayerData curPlayerData = model.AllPlayerData[_currentCharIndex];
            PlayerSkin curPlayerSkin = curPlayerData.PlayerSkin;
            //데이터 유지를 위해 객체를 생성하지않고 material color설정
            curPlayerSkin.FaceMaterial.color = newColor;
            curPlayerSkin.BodyMaterial.color = newColor;
        }
    }
}
