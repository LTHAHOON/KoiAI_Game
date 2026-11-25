# Koi AI 프로젝트 에이전트 지침

이 문서는 Koi AI Unity 프로젝트의 코드, 씬 하이어라키, UI 참조, 에셋 연결 방식을 같은 스타일로 유지하기 위한 작업 기준이다. 새로운 기능은 이 문서의 구조와 기존 구현을 먼저 확인한 뒤 추가한다.

## 프로젝트 기본 정보

- Unity 버전은 6000.4.0f1이다.
- 주 게임 코드는 Assets/_KoiAI_Project/Assets 아래에 있다.
- 주요 씬은 Assets/_KoiAI_Project/Scenes/LoadingScreenScene, LobbyScene, InGameScene, StoryScene이다.
- 렌더 파이프라인은 URP다.
- 입력은 Unity Input System과 InputService를 통해 처리한다.
- 비동기 흐름은 프로젝트에 이미 사용 중인 UniTask와 R3를 우선한다.
- 카메라는 Cinemachine을 사용한다.
- UI는 UI Toolkit과 Unity Canvas UI를 함께 사용한다.
- 프로젝트에는 Unity MCP 패키지 com.coplaydev.unity-mcp가 포함되어 있다.

## 작업 범위와 폴더 구조

런타임 스크립트는 기능에 맞는 폴더 아래에 둔다. 새 기능을 만들 때 기존 기능 폴더와 네임스페이스를 재사용한다.

| 폴더 | 역할 |
| --- | --- |
| Scripts/Core | 게임 매니저, 저장, 엔티티 식별자, 공통 게임 이벤트 |
| Scripts/Input | InputService, Action Map 전환, 생성된 Input Action 래퍼 |
| Scripts/UI | Canvas UI, HUD, 모니터, 팝업, 화면 전환 |
| Scripts/UXML | UI Toolkit 화면의 Model, ViewInfo, View, Presenter |
| Scripts/Pool | 풀, 풀 관리자, 팝업 컨테이너 |
| Scripts/AI | AI Brain, Feature, 상태, 조건, 이동, 공격 |
| Scripts/Health | 체력, 체력바, 사망 처리 |
| Scripts/Inventory | 아이템, 무기, 슬롯, 인벤토리 |
| Scripts/Interact | 상호작용 대상과 상호작용 조건 |
| Scripts/Quest | 퀘스트 데이터, Objective Controller, View, 이벤트 |
| Scripts/VFX | 총알 트레이서와 VFX 제어 |
| Scripts/Utilities | 씬 참조, 페이드, UI 추적, 공통 보조 기능 |

- Assets/PracticeProject는 실험 및 학습용 코드다. 요청이 없으면 기능 수정 대상에서 제외한다.
- Assets/_Recovery는 Unity 자동 복구 산출물이다. 요청이 없으면 수정하거나 정리하지 않는다.
- Editor 전용 코드는 Editor 폴더 또는 Editor asmdef 범위에 둔다.
- 새 Unity 에셋을 추가하거나 이동할 때 .meta 파일을 함께 유지한다.

## 코드 스타일

### 클래스와 네임스페이스

- 네임스페이스는 KoiAI를 기준으로 기능 단위로 나눈다. 예를 들어 KoiAI.UI, KoiAI.Input, KoiAI.Quest, KoiAI.AI를 사용한다.
- 클래스는 하나의 책임을 중심으로 만든다. 데이터는 ScriptableObject 또는 직렬화 데이터 클래스로, 동작은 MonoBehaviour로 둔다.
- 화면 표시와 UI 요소 검색은 View 계층에, 화면 흐름과 도메인 호출은 Presenter 또는 Controller에 둔다.
- 공통 동작을 확장할 때는 기존 추상 클래스와 인터페이스를 우선 재사용한다. 같은 목적의 전역 관리자나 서비스 클래스를 새로 만들지 않는다.

### 이름과 필드

- private 필드는 _camelCase로 작성하고 Inspector에 연결할 필드에는 [SerializeField]를 사용한다.
- public 클래스, 메서드, 프로퍼티, 이벤트는 PascalCase를 사용한다.
- 이벤트 핸들러는 On... 이름을 사용한다. 예를 들어 OnClickPlayButton, OnBeginOpen, OnCompleteClose처럼 작성한다.
- Bool 필드는 _is, _b, Has, Can 계열 기존 표현을 따른다.
- enum 값은 기능의 현재 표현을 유지한다. 문자열로 Action Map을 찾는 코드가 있으므로 enum 이름도 임의로 바꾸지 않는다.
- 기존 직렬화 필드명, 클래스명, 네임스페이스, ScriptableObject GUID, 씬·프리팹 참조는 임의로 변경하지 않는다.
- 기존 코드에 Initalize, Lobbdy, Weaing처럼 오탈자가 있어도 직렬화와 호출 호환성 때문에 새 코드에서 기존 API를 사용할 때는 같은 이름을 사용한다.
- 컬렉션은 초기화 시점과 소유자를 명확히 한다. 현재 프로젝트처럼 new()로 생성하고 수명 종료 시 정리하는 패턴을 우선한다.

### 이벤트와 수명

- C# event, UnityEvent, R3 Subject를 구독하면 객체가 파괴될 때 반드시 해제하거나 AddTo(this)로 수명을 묶는다.
- OnEnable에서 등록한 이벤트는 OnDisable에서 해제한다. Awake 또는 Start에서 등록한 이벤트는 OnDestroy에서 해제한다.
- DOTween Tween은 중복 실행 전 기존 Tween을 종료하고, 객체가 파괴되면 함께 종료되도록 연결한다.
- 전역 Singleton은 기존 Instance 패턴을 재사용한다. 새 전역 상태는 씬 전환과 중복 초기화를 검토한 뒤 추가한다.

### 비동기와 업데이트

- 씬 전환은 AsyncSceneChanger와 IAsyncSceneChangeHandler를 사용한다.
- 반복 관찰은 R3 Observable을 우선하고, 비동기 작업은 UniTask를 우선한다.
- Update 폴링은 매 프레임 갱신이 꼭 필요한 경우에만 사용한다. 입력 이벤트, R3, UniTask로 대체할 수 있으면 대체한다.
- 코루틴이나 UniTask가 중단될 때 오브젝트가 파괴되거나 씬이 바뀌는 상황을 처리한다.

## 씬 하이어라키와 참조 구성

씬과 프리팹은 코드에서 검색해 복구하기보다 Inspector 직렬화 참조로 연결하는 것이 기본이다.

### LobbyScene

- LobbyPresenter는 UIDocument, LobbyViewInfo, 팝업 목록, 씬 참조, 종료 Timeline을 Inspector에서 받는다.
- LobbyPresenter.Initalize는 LobbyView를 만들고 UXML 버튼 이벤트를 연결한 뒤 InputService.SetEnableActionMap(InputActionMapContext.Lobbdy)를 호출한다.
- 캐릭터 설정은 PopUpManager가 LobbyPresenter의 _popUpWindows 목록에서 CharacterSettingWindow를 찾아 연다.
- CharacterSettingWindow는 PopUpWindow_UID<CharacterSettingModel, CharacterSettingViewInfo>를 상속한다. Model, ViewInfo, UIDocument Container, 미리보기 캐릭터 부모를 Inspector에서 연결한다.
- Lobby 종료는 PlayableDirector Signal에서 Presenter의 공개 메서드를 호출하고, 그 안에서 비동기 씬 전환을 시작한다.

### InGameScene

- UIManager는 Base Canvas와 Bloom Canvas를 보유한다. 화면 좌표를 UI 좌표로 변환해야 할 때 기존 ViewPortToAnchoredPoint를 사용한다.
- Canvas UI는 기능별 루트 오브젝트 아래에 두고, CanvasGroup, Button, TMP_Text, Image, Slider 등의 참조를 컴포넌트에 직렬화한다.
- MonitorScreenView는 부모 Canvas 팝업이며 Login, Hacking, Reward 같은 자식 팝업을 배열로 관리한다.
- PoolManager는 씬에서 초기화하고 퀘스트 Objective, 해킹 파티클 등 반복 생성되는 객체는 EntityID와 PoolSize를 이용해 풀에 등록한다.
- InputService는 씬에 존재하지만 DontDestroyOnLoad로 유지된다. 중복 생성과 Action Map 상태를 확인한다.

### 이름과 연결

- UXML 요소의 name은 VisualViewInfo의 문자열 필드와 root.Q<T>() 검색으로 연결된다. UXML 이름을 바꾸면 ViewInfo와 코드를 함께 수정한다.
- Canvas 오브젝트 이름과 prefab 계층은 Inspector 참조와 Timeline Signal의 대상이 될 수 있으므로 임의로 바꾸지 않는다.
- 씬, 프리팹, UXML, USS, ScriptableObject는 참조 방향을 유지한다. 런타임 검색으로 누락 참조를 조용히 대체하지 말고 초기화 시 null을 검사하고 원인을 로그로 확인한다.
- .unity, .prefab, .asset을 직접 대량 텍스트 수정하지 않는다. 가능하면 Unity Editor 또는 Unity MCP로 변경한다.

## UI Toolkit MVP 구조

현재 UI Toolkit 화면은 VisualModel, VisualViewInfo, VisualView, VisualPresenter를 기본 단위로 사용한다.

- VisualModel은 ScriptableObject이며 화면이 사용할 데이터와 상태를 보유한다.
- VisualViewInfo는 UXML 이름, 템플릿, 반복 버튼 지연 시간 같은 에디터 설정을 보유한다.
- VisualView<TInfo>는 VisualElement root를 받고 Initalize에서 요소를 검색해 private 필드에 캐시한다.
- VisualPresenter<TView, TViewInfo>는 MonoBehaviour이며 UIDocument와 ViewInfo를 받아 View를 생성하고 버튼 이벤트와 화면 흐름을 연결한다.
- View는 root.Q<T>()와 ViewInfo의 이름을 사용한다. 화면 전환, 저장, 씬 전환, 도메인 서비스 호출은 Presenter 또는 Window가 담당한다.
- 반복되는 UXML 슬롯은 CloneTree()로 복제하고, 데이터 객체를 연결한 뒤 부모 ScrollView에 추가한다. 생성한 슬롯의 이벤트와 표시 상태를 함께 초기화한다.
- UI Toolkit Button, RepeatButton 이벤트는 초기화 시 한 번만 등록하고 객체 수명 종료 시 해제한다.

## 인게임 팝업 UI 규칙

팝업은 기존 PopUpManager가 관리하는 대상이어야 한다. 팝업 GameObject를 직접 켜고 끄거나 새로운 전역 팝업 스택을 만들지 않는다.

### 렌더 방식 선택

- UXML과 USS로 구성된 화면은 PopUpWindow_UID 계열을 사용한다.
- Canvas, CanvasGroup, DOTween 페이드, Unity UI Button을 사용하는 화면은 PopUpWindow_Canvas 계열을 사용한다.
- 부모 팝업 안에서 화면을 교체하는 기능은 부모 하나와 자식 PopUpWindow_Canvas 배열을 구성한다.
- 기존 Canvas와 UIDocument 계층을 우선 사용한다. 팝업마다 별도의 Canvas, EventSystem, InputService를 새로 만들지 않는다.

### 공통 상태 흐름

PopUpManager.ChangePopUpState가 상태 변경의 단일 진입점이다.

1. 호출자가 Window 참조를 얻는다.
2. ChangePopUpState(PopUpState.Open, window)으로 연다.
3. Manager가 Container의 현재 상태를 확인한다.
4. OnBeginOpen 또는 OnBeginClose를 호출한다.
5. UIDocument는 VisualElement.visible, Canvas는 GameObject.SetActive 또는 CanvasGroup Tween을 변경한다.
6. Manager가 Stack을 새로 고친다.
7. 완료 시 OnCompleteOpen 또는 OnCompleteClose를 호출한다.
8. Global Cancel 입력은 Stack 최상단 Window를 닫는다.

동일 상태로 다시 요청하면 아무 작업도 하지 않도록 기존 Manager의 중복 방지 동작을 유지한다.

### UIDocument 팝업 구성

다음 참조를 Inspector에서 연결한다.

- PopUpWindow_UID<TModel, TViewInfo>를 상속한 Window 컴포넌트
- Window의 _visualModel
- Window의 _viewInfo
- PopUpWindowContainer<UIDocument, string>의 Parent UIDocument
- Container의 Window UXML 루트 이름
- 호출 Presenter의 _popUpWindows 배열

구현 규칙은 다음과 같다.

- Initalize(TModel visualModel, TViewInfo viewInfo)에서 Container Parent의 rootVisualElement를 얻는다.
- Container의 Window 이름으로 팝업 루트를 검색한다.
- viewInfo에 저장한 이름으로 Button, Label, ScrollView 등을 검색하고 필드에 캐시한다.
- 버튼 콜백은 Initalize에서 연결한다.
- Confirm, Cancel, Close 동작은 PopUpManager를 통해 상태를 변경한다.
- Model의 데이터 변경은 Model 또는 도메인 서비스에 맡기고 View는 표시만 담당한다.
- 팝업이 다시 열릴 때 이전 선택 상태와 동적 슬롯이 남지 않는지 OnBeginOpen 또는 명시적인 초기화 함수에서 확인한다.
- 이벤트 구독은 OnDestroy에서 해제한다.

### Canvas 팝업 구성

다음 참조를 Inspector에서 연결한다.

- PopUpWindow_Canvas를 상속한 Window 컴포넌트
- PopUpWindowContainer<Transform, GameObject>의 Parent Transform
- Container의 Window prefab 또는 하이어라키 GameObject
- CanvasGroup과 CanvasGroupFadeSettings가 있다면 TryGetFadeSettings가 반환하도록 구성
- 자식 팝업이 있다면 _childPopUpWindows 배열
- 가려야 하는 HUD가 있다면 _hidingCanvasGroups 배열

구현 규칙은 다음과 같다.

- Window의 직접 SetActive 호출은 초기화와 Manager 내부 동작에만 맡긴다. 외부 호출자는 Manager를 사용한다.
- 페이드가 필요한 Window는 TryGetFadeSettings를 override한다.
- 열기 전에는 OnBeginOpen, 닫기 전에는 OnBeginClose를 사용한다.
- 애니메이션 종료 뒤 처리는 OnCompleteOpen, OnCompleteClose에 작성한다.
- 부모 닫기 전에 자식 팝업을 순서대로 닫는다.
- 부모가 숨긴 HUD CanvasGroup은 닫힘 처리에서 다시 연다.
- Popup이 입력을 독점할 때는 필요한 Action Map을 InputService.SetEnableActionMap으로 전환하고 닫을 때 기존 상태를 복구한다.
- 새 페이드 Tween은 PopUpManager.ChangeCanvasGroupState를 우선 사용한다. 직접 DOTween을 만들면 기존 Tween 종료, unscaled time, GameObject 파괴 연결을 동일하게 처리한다.

### 부모와 자식 팝업

MonitorScreenView 방식처럼 부모 Window가 자식 Window 목록을 소유한다.

- OpenChildView<T>()는 요청한 타입을 열고 나머지 자식은 닫는다.
- 자식 Window는 부모 Canvas 또는 동일한 팝업 계층 아래에 둔다.
- 부모 Window의 OnBeginClose에서 모든 자식을 닫는다.
- 자식이 닫혔을 때 부모까지 자동으로 닫을지 여부는 호출 흐름에서 명시한다.
- 자식마다 별도의 Global Cancel 처리를 만들지 않는다. Manager Stack이 최상단 자식을 먼저 닫는다.

### 호출 코드 패턴

호출자는 GetPopUpWindows로 목록을 얻고 FindPopUpWindow<T>()로 Window를 찾은 뒤 ChangePopUpState를 호출한다.

    private void OnClickOpenExample()
    {
        PopUpWindow[] popUpWindows = GetPopUpWindows();
        ExamplePopupWindow window =
            PopUpManager.Instance.FindPopUpWindow<ExamplePopupWindow>(popUpWindows);
        PopUpManager.Instance.ChangePopUpState(PopUpState.Open, window);
    }

UIDocument Window는 Container Parent의 rootVisualElement에서 Container Window 이름으로 root를 검색하고 ViewInfo 이름으로 컨트롤을 캐시한다. 닫기 버튼은 PopUpManager를 통해 닫고 OnDestroy에서 콜백을 해제한다.

Canvas Window는 PopUpWindow_Canvas를 상속하고 CanvasGroup과 CanvasGroupFadeSettings를 TryGetFadeSettings에서 반환한다. OnBeginOpen에서 필요한 Action Map을 켜고 OnBeginClose에서 자식 팝업과 임시 입력 상태를 정리한다.

## 새 인게임 팝업 제작 순서

1. UXML 방식인지 Canvas 방식인지 결정한다.
2. 팝업이 표시될 씬의 Canvas 또는 UIDocument와 부모 하이어라키를 확인한다.
3. UXML 루트 이름, 버튼 이름, Label 이름 또는 prefab 자식 구조를 먼저 확정한다.
4. Model, ViewInfo, View, Presenter 또는 Window의 책임을 나눈다.
5. 필요한 .cs, .uxml, .uss, prefab, ScriptableObject를 기능 폴더에 추가한다.
6. 모든 Inspector 참조를 연결한다. Parent, Window, UIDocument, ViewInfo, Model, CanvasGroup 누락을 남기지 않는다.
7. 호출 화면의 _popUpWindows 배열에 Window를 추가한다.
8. 호출자는 FindPopUpWindow<T>()로 찾은 뒤 ChangePopUpState를 호출한다.
9. Cancel, Confirm, Close, 부모-자식 전환, Action Map 전환을 연결한다.
10. 이벤트와 Tween의 해제 경로를 작성한다.
11. 팝업을 열고 닫은 뒤 다시 열어 상태, 동적 슬롯, 입력, Stack 순서가 정상인지 확인한다.
12. 컴파일 오류와 Console 경고를 확인하고 씬을 저장한다.

## 재사용 가능한 구현 패턴

- 데이터가 에디터에서 설정되고 런타임에 여러 화면에서 공유되면 ScriptableObject를 사용한다.
- 씬에 하나만 존재하며 여러 시스템이 접근하는 서비스는 기존 Singleton 수명과 초기화 방식을 따른다.
- 반복 생성되는 UI 또는 VFX는 PoolManager, PoolSize, EntityID 패턴을 사용한다.
- 상호작용·퀘스트·체력처럼 여러 기능이 반응하는 사건은 기존 이벤트 또는 Subject 흐름을 사용한다.
- 화면 좌표 추적은 UIFollowToObject, Canvas 좌표 변환은 UIManager를 재사용한다.
- 페이드와 화면 전환은 CanvasGroupFadeSettings, PopUpManager, AsyncSceneChanger를 재사용한다.
- 데이터 변경과 표시 갱신을 한 클래스에 몰아넣지 않는다. Quest Controller와 Quest View처럼 상태 계산과 표시를 분리한다.
- 같은 종류의 데이터 목록은 기존 List, Dictionary, 직렬화 배열 사용 방식을 유지하고, 임의의 새로운 저장 포맷을 만들지 않는다.

## Unity 작업 절차

Unity Editor 또는 Unity MCP를 사용할 때 다음 순서를 지킨다.

1. Editor 상태와 현재 씬, 대상 GameObject, 연결된 프로젝트를 확인한다.
2. Unity MCP를 사용한다면 mcpforunity://project/info의 projectRoot가 현재 저장소와 일치하는지 확인한다. 현재 저장소 경로는 C:/Users/KCH/Desktop/KoiAI_Game이다. 다른 프로젝트가 연결되어 있으면 변경 작업을 중단한다.
3. 스크립트를 만들거나 수정한 뒤 컴파일이 끝날 때까지 기다린다.
4. Unity Console의 컴파일 오류와 경고를 확인한다.
5. 씬, 프리팹, UI, VFX를 변경했다면 가능한 경우 Play Mode 또는 관련 테스트로 동작을 확인하고 화면 결과를 캡처한다.
6. 팝업은 열기, 닫기, Global Cancel, 재열기, 중첩 팝업, 입력 Action Map, 씬 재진입을 확인한다.
7. 변경된 파일과 검증 결과, 검증하지 못한 항목을 최종 보고에 명시한다.

## 검증 명령

    # 변경 상태 확인
    git status --short

    # 스크립트 목록 확인
    rg --files Assets/_KoiAI_Project/Assets/Scripts -g '*.cs'

    # 팝업 및 UI 참조 확인
    rg -n "PopUpWindow|PopUpManager|VisualPresenter|UIDocument|InputService" Assets/_KoiAI_Project/Assets/Scripts

    # 문서 공백 검사
    git diff --check

    # Unity가 설치된 환경에서 EditMode 테스트
    Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Temp/editmode-results.xml -quit

Unity MCP를 사용할 수 없으면 저장소 파일을 최소 범위로 수정하고 Unity Editor에서 같은 검증을 수행한다. MCP 인스턴스가 다른 프로젝트를 가리키면 연결이 교체될 때까지 MCP 변경을 수행하지 않는다.

## 변경 및 Git 규칙

- 사용자가 요청한 범위만 수정한다.
- 이미 작업 중인 변경 사항을 되돌리거나 덮어쓰지 않는다.
- 커밋, 리셋, 대량 삭제 같은 파괴적 Git 작업은 요청 없이는 실행하지 않는다.
- 기능과 무관한 포맷 변경, 파일 이동, 이름 변경, 외부 패키지 추가를 하지 않는다.
- 직렬화 필드와 에셋 GUID를 보존해 기존 씬과 프리팹 참조가 유지되도록 한다.

## 최종 보고 형식

작업을 마칠 때 다음 항목을 간단히 보고한다.

- 변경한 파일
- 구현한 기능 또는 문서화한 규칙
- 실행한 검증 명령과 결과
- Unity Editor, Play Mode, MCP 검증 여부
- 남은 컴파일·런타임 위험 또는 확인하지 못한 항목
