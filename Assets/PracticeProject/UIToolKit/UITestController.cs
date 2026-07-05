using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using TMPro.EditorUtilities;

public class UITestController : MonoBehaviour
{
    private VisualElement _bottomContainer;

    private Button _openButton;
    private Button _closeButton;

    private VisualElement _bottomSheet;
    private VisualElement _scrimSheet;
    private VisualElement _boy;
    private VisualElement _girl;

    //대사용 레이블
    private Label _message;
    void Start()
    {
         var root = GetComponent<UIDocument>().rootVisualElement;
         _bottomContainer = root.Q<VisualElement>("Container_Bottom");
         _openButton = root.Q<Button>("Button_Open");
         _closeButton = root.Q<Button>("Button_Close");
         _bottomSheet = root.Q<VisualElement>("BottomSheet");
         _scrimSheet = root.Q<VisualElement>("Scrim");
         _boy = root.Q<VisualElement>("Image_Boy");
         _girl = root.Q<VisualElement>("Image_Girl");
         _message = root.Q<Label>("Description");
         //시작할 때 팝업 그룹을 감춘다.
         _bottomContainer.style.display = DisplayStyle.None;
         //_openButton.clicked += () => _bottomContainer.style.display = DisplayStyle.Flex;
         //_closeButton.clicked += () => _bottomContainer.style.display = DisplayStyle.None;
         _openButton.RegisterCallback<ClickEvent>(OnOpenButtonClick);
         _closeButton.RegisterCallback<ClickEvent>(OnCloseButtonClick);
         _bottomSheet.RegisterCallback<TransitionEndEvent>(OnBottomSheetDown);
         Invoke("AnimateBoy", 2f);
    }

    private void Update()
    {
     //   Debug.Log(_boy.ClassListContains("image--boy--inair"));
    }
    
    private void AnimateBoy()
    {
        _boy.RemoveFromClassList("image--boy--inair");
    }

    private void AnimateGirl()
    {
        _girl.AddToClassList("image--girl--up");
        //Transition이 끝나면 삭제하고 다시 추가
        _girl.RegisterCallback<TransitionEndEvent>(_ => _girl.ToggleInClassList("image--girl--up"));
        _message.text = string.Empty;
        string message = "안녕하세요 저는 이태훈이라고 합니다.";
        DOTween.To(() => _message.text, x => _message.text = x, message, 3f).SetEase(Ease.Linear);
    }
    
    private void OnOpenButtonClick(ClickEvent evt)
    {
        _bottomContainer.style.display = DisplayStyle.Flex;
        _bottomSheet.AddToClassList("bottomsheet--up");
        _scrimSheet.AddToClassList("scrim--fadein");
         AnimateGirl();
    }

    private void OnBottomSheetDown(TransitionEndEvent evt)
    {
        if (!_bottomSheet.ClassListContains("bottomsheet--up"))
        {
            _bottomContainer.style.display = DisplayStyle.None;
        }
        
    }
    
    private void OnCloseButtonClick(ClickEvent evt)
    {
        _bottomSheet.RemoveFromClassList("bottomsheet--up");
        _scrimSheet.RemoveFromClassList("scrim--fadein");
    }
}
