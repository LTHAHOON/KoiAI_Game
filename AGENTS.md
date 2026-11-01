# Koi AI 프로젝트 에이전트 지침

## 프로젝트 개요

- Unity `6000.4.0f1` 프로젝트입니다.
- 주 게임 코드는 `Assets/_KoiAI_Project/Assets` 아래에 있습니다.
- 주요 씬은 `Assets/_KoiAI_Project/Scenes/`의 `LoadingScreenScene`, `LobbyScene`, `InGameScene`, `StoryScene`입니다.
- 렌더 파이프라인은 URP이며, 입력은 Input System, 비동기 코드는 UniTask/R3, 카메라는 Cinemachine을 사용합니다.
- 프로젝트에 Unity MCP 패키지(`com.coplaydev.unity-mcp`)가 포함되어 있습니다.
- Unity MCP를 사용할 때 `mcpforunity://project/info`의 `projectRoot`가 현재 저장소 경로(`C:/Users/82108/Documents/Unity Games/KoiAI_Game`)와 일치하는지 먼저 확인합니다. 다른 프로젝트가 연결되어 있으면 MCP 변경 작업을 중단합니다.

## 작업 범위와 구조

- `Assets/_KoiAI_Project/Assets/Scripts/AI`: AI 브레인, 상태/조건, 이동 및 공격 로직
- `Assets/_KoiAI_Project/Assets/Scripts/Input`: 입력 서비스와 Action Map 전환
- `Assets/_KoiAI_Project/Assets/Scripts/Health`: 체력, 체력바, 사망 처리
- `Assets/_KoiAI_Project/Assets/Scripts/Inventory`: 아이템/무기/슬롯 시스템
- `Assets/_KoiAI_Project/Assets/Scripts/UXML`: UI Toolkit 기반 Lobby/UI 코드
- `Assets/_KoiAI_Project/Assets/Scripts/VFX`: VFX 및 총알 트레이서 로직
- `Assets/PracticeProject`: 실험 및 학습용 코드입니다. 기능 수정의 기본 대상이 아닙니다.
- `Assets/_Recovery`: Unity 자동 복구 산출물입니다. 요청이 없으면 수정하거나 정리하지 않습니다.

## 변경 규칙

1. 사용자가 요청한 범위만 수정하고, 이미 작업 중인 변경 사항은 되돌리거나 덮어쓰지 않습니다.
2. Unity 직렬화 파일(`.unity`, `.prefab`, `.asset`)을 직접 텍스트로 대량 수정하지 말고, 가능하면 Unity Editor 또는 Unity MCP를 사용합니다.
3. 새 Unity 에셋을 추가하거나 이동할 때 `.meta` 파일을 함께 유지합니다.
4. 런타임 스크립트는 `Assets/_KoiAI_Project/Assets/Scripts` 안의 기능별 폴더에 배치합니다. Editor 전용 코드는 `Editor` 폴더 또는 Editor asmdef 범위에 둡니다.
5. 기존 코드의 네임스페이스, 직렬화 필드명, ScriptableObject GUID, 씬/프리팹 참조를 임의로 바꾸지 않습니다.
6. 입력은 `InputService`를 통해 처리하고, UI Toolkit 화면은 기존 MVP 구조(View/Presenter/Model)를 따릅니다.
7. 비동기 흐름은 프로젝트에 이미 사용 중인 UniTask/R3 패턴을 우선하며, `Update` 폴링이나 새 외부 의존성 추가는 필요성을 확인한 뒤 사용합니다.
8. 커밋/리셋/대량 삭제 같은 파괴적 Git 작업은 요청 없이는 실행하지 않습니다.

## Unity 작업 절차

Unity Editor 또는 Unity MCP를 사용할 때 다음 순서를 지킵니다.

1. Editor 상태와 현재 씬/대상 GameObject를 먼저 확인합니다.
2. 스크립트를 만들거나 수정한 뒤 컴파일이 끝날 때까지 기다립니다.
3. Unity Console의 컴파일 오류와 경고를 확인합니다.
4. 씬, 프리팹, UI, VFX를 변경했다면 가능한 경우 Play Mode 또는 관련 테스트로 동작을 확인하고 화면 결과를 캡처합니다.
5. 변경된 파일과 검증 결과를 최종 보고에 명시합니다.

## 검증 명령

저장소 루트에서 실행합니다.

```powershell
# 변경 상태 확인
git status --short

# C# 파일 목록 및 잠재적인 컴파일 대상 확인
rg --files Assets/_KoiAI_Project/Assets/Scripts -g '*.cs'

# Unity가 설치된 환경에서는 Editor의 batchmode 테스트를 사용합니다.
# 실제 Unity.exe 경로는 개발 환경에 맞게 지정합니다.
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Temp/editmode-results.xml -quit
```

Unity MCP를 사용할 수 있으면 파일을 직접 조작하기보다 Editor 상태 조회, 대상 확인, 변경, 컴파일 대기, Console 확인 순으로 진행합니다. MCP 도구를 사용할 수 없는 경우에는 저장소 파일을 최소 범위로 수정하고 Unity Editor에서 동일한 검증을 수행합니다. MCP 인스턴스가 다른 프로젝트를 가리키면 연결이 교체될 때까지 MCP 변경을 수행하지 않습니다.

## 보고 형식

작업을 마칠 때 변경한 파일, 실행한 검증, 남은 컴파일/런타임 위험을 간단히 보고합니다. 검증하지 못한 항목은 통과한 것처럼 표현하지 않습니다.
