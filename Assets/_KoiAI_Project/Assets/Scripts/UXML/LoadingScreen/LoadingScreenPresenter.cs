using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KoiAI.UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    using DG.Tweening;
    using KoiAI.Utilities;
    using R3;
    using UnityEngine.SceneManagement;

    public class LoadingScreenPresenter : VisualPresenter<LoadingScreenView, LoadingScreenViewInfo>
    {
        [SerializeField]
        private PlayableDirector _exitLoadingDirector;
        [SerializeField]
        private PlayableAsset _exitLoadingTimeline;
        [SerializeField]
        private UIScaleTransitionData _gameIconScaleTransitionData;
        [Header("로드할 씬")]
        [SerializeField]
        private SceneReference _loadSceneReference;
         
        private UIScaleTransition _gameIconScaleTransition;
        
        protected override void Initalize(UIDocument uiDoucument, ref LoadingScreenView visualView, LoadingScreenViewInfo visualViewInfo)
        {
            visualView = new(uiDoucument.rootVisualElement, visualViewInfo);
            _gameIconScaleTransition = new(gameObject, visualView.GameIconImage,_gameIconScaleTransitionData);
            _gameIconScaleTransition.ActivateTransition();
            
            //드래그를 막기 위해 BubbleUp(버블단계)가 아닌 TrickleDown(캡쳐단계)에서 이벤트가 호출되도록합니다.
            //드래그 이벤트는 BubbleUp단계에서 실행되기 때문에 이보다 더 빠른 단계인 TrickleDown단계에서 해야합니다.
            visualView.LoadingSlider.RegisterCallback<PointerDownEvent>(e =>
            {
                //해당 UI의 모든 이벤트처리를 막아줍니다.(StopPropagation같은 경우 사용자가 등록한 이벤트는 막지 못합니다.)
                e.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);
        }

        //TODO: FadeOut-End 타임라인 시그널로 인해 호출되어 목표 씬으로 전환합니다.
        public void OnEndLoadingScreenEnter_Signal()
        {
            if (_loadSceneReference == null)
            {
                Debug.LogError("Error: LoadSceneReference is null");
            }

            LoadingScreenView visualView = GetVisualView();
            CancellationToken cancellationToken = gameObject.GetCancellationTokenOnDestroy();
            
            //로딩 화면에서 인게임 화면으로 갈 때는 메모리를 줄이기 위해서 Unload를 해줍니다.(Single대신 Additive + Unload 사용)
            IAsyncSceneChangeHandler sceneChangeHandler =  AsyncSceneChanger.CreateAsyncSceneLoadHandler();
            sceneChangeHandler
                .SetLoadSceneMode(LoadSceneMode.Additive)
                .SetLoadDelayTime(0.7f)
                .SetProgressSubscribe(pct =>
                {
                    visualView.LoadingSlider.value = pct;
                }, cancellationToken)
                .StartChangeScene(this, _loadSceneReference, true);
        }
        
    }
    
}
