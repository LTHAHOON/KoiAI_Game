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
        private SceneReference _loadSceneReference;
        [SerializeField]
        private PlayableDirector _exitLobbyDirector;
        [SerializeField]
        private PlayableAsset _exitLobbyTimeline;
        
        protected override void Initalize(UIDocument uiDoucument, ref LobbyView visualView, LobbyViewInfo visualViewInfo)
        {
            visualView = new LobbyView(uiDoucument.rootVisualElement, visualViewInfo);
            
            visualView.PlayButton.clicked += OnClickPlayButton;
            visualView.CharacterSettingButton.clicked += OnClickCharacterSettingButton;
        }

        private void OnClickPlayButton()
        {
            _exitLobbyDirector.Play(_exitLobbyTimeline);
        }
        
        private void OnClickCharacterSettingButton()
        {
            
        }

        //TODO: FadeIn-End 타임라인 시그널로 인해 호출되어 로딩화면 씬으로 전환합니다. 
        public void OnEndLobbyExit_Signal()
        {
            IAsyncSceneChangeHandler sceneChangeHandler =  AsyncSceneChanger.CreateAsyncSceneLoadHandler();
            sceneChangeHandler
                .SetLoadSceneMode(LoadSceneMode.Additive)
                .AssignOnLoadCompleted(OnLoadCompleted)
                .StartChangeScene(this, _loadSceneReference, true);
        }

        private void OnLoadCompleted(Scene prevScene)
        {

        }
    }
}
