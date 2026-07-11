using System;
using R3;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace KoiAI.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private TimelineAsset _startCutSceneTimeline;
        [SerializeField]
        private PlayableDirector _playableDirector;
        [SerializeField]
        private PlayerInput _playerInput;
    
        private Subject<PlayableDirector> _cutSceneSubject = new(); 
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _cutSceneSubject
                .Subscribe(playableDirector =>
                {
                    Observable.EveryUpdate()
                        .Where(_ => playableDirector && playableDirector.state == PlayState.Paused)
                        .Take(1)
                        .Subscribe(_ => EndCutScene());
                })
                .AddTo(this);
        }

        private void Start()
        {
            GameStart();
        }
        
        private void GameStart()
        {
            if (!_playableDirector || !_startCutSceneTimeline)
            {
                return;
            }
            _playableDirector.Play();
            PlayCutScene(_startCutSceneTimeline);
        }

        private void PlayCutScene(TimelineAsset timelineAsset)
        {
            if (!_playerInput)
            {
                return;
            }
            _playerInput.enabled = false;
            _playableDirector.Play(timelineAsset);
            _cutSceneSubject.OnNext(_playableDirector);
        }
    
        private void EndCutScene()
        {
            if (!_playerInput)
            {
                return;
            }
            _playerInput.enabled = true;
        }
    
    }   

}
