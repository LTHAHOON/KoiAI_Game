using System.Collections.Generic;
using KoiAI.Core;
using KoiAI.Utilities;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyPresenter : VisualPresenter<LobbyView, LobbyViewInfo>
    {
        [SerializeField]
        private List<GameObject> _activeObjsForExit;
        
        [SerializeField]
        private SceneReference _loadSceneReference;
        [SerializeField]
        private PlayableDirector _exitLobbyDirector;
        [SerializeField]
        private PlayableDirector _enterLobbyDirector;
        [SerializeField]
        private PlayableAsset _exitLobbyTimeline;
        [SerializeField]
        private PlayableAsset _enterLobbyTimeline;
        
        protected override void Initalize(UIDocument uiDoucument, ref LobbyView visualView, LobbyViewInfo visualViewInfo)
        {
            visualView = new LobbyView(uiDoucument.rootVisualElement, visualViewInfo);
            
            visualView.PlayButton.clicked += OnClickPlayButton;
        }

        private void OnClickPlayButton()
        {
            _exitLobbyDirector.Play(_exitLobbyTimeline);
        }
        
        //TODO: 타임라인 시그널로 인해 호출되어 로딩화면 씬으로 전환합니다. 
        public void OnStartLobbyExit_Signal()
        {
            IAsyncSceneChangeHandler sceneChangeHandler =  AsyncSceneChanger.CreateAsyncSceneLoadHandler();
            sceneChangeHandler
                .SetLoadSceneMode(LoadSceneMode.Additive)
                .AssignOnLoadCompleted(OnLoadCompleted)
                .StartChangeScene(this, _loadSceneReference, true);
        }
        
        public void OnEndLobbyExit_Signal()
        {

        }

        private void OnLoadCompleted(Scene notActiveScene)
        {

        }
    }
}
