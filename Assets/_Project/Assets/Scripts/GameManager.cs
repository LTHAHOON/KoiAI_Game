using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TimelineAsset _startCutSceneTimeline;
    [SerializeField]
    private PlayableDirector _playableDirector;
    [SerializeField]
    private PlayerInput _inputSystem;
    
    private Subject<PlayableDirector> _cutSceneSubject = new(); 
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _cutSceneSubject
            .Subscribe(playableDirector =>
            {
                Observable.EveryUpdate()
                    .Where(_ => playableDirector.state == PlayState.Paused)
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
        _playableDirector.Play();
        PlayCutScene(_startCutSceneTimeline);
    }

    private void PlayCutScene(TimelineAsset timelineAsset)
    {
        _inputSystem.enabled = false;
        _playableDirector.Play(timelineAsset);
        _cutSceneSubject.OnNext(_playableDirector);
    }
    
    private void EndCutScene()
    {
        _inputSystem.enabled = true;
    }
    
}   
