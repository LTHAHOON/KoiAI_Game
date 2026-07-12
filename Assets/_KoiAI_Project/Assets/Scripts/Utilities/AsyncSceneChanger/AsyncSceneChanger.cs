using System;
using R3;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KoiAI.Utilities
{
    //확장클래스와 함수명 겹치는 문제와 은닉화를 위해 인터페이스 활용 
    public interface IAsyncSceneChangeHandler 
    {
        /// <summary>
        /// 비동기 로드 실행
        /// </summary>
        public void StartChangeScene(MonoBehaviour mono, SceneReference sceneReference, bool bUnloadPrevScene);
        
        /// <summary>
        /// 진행률(0~100%)
        /// </summary>
        public ReactiveProperty<float> CurrentLoadProgressPct { get; }
    }

    public class AsyncSceneChanger : IAsyncSceneChangeHandler
    {
        private readonly ReactiveProperty<float> _curProgressPct = new(0);
        private LoadSceneMode _loadSceneMode = LoadSceneMode.Single;
        private float _loadDelayTime = 0f;
        private AsyncOperation _asyncOperation;
        /// <summary>
        /// Parameter: Scene Previous NotActiveScene
        /// </summary>
        public Action<Scene> OnLoadCompleted;
        
        public static IAsyncSceneChangeHandler CreateAsyncSceneLoadHandler()
        {
            IAsyncSceneChangeHandler changeHandler = new AsyncSceneChanger();
            return changeHandler;
        }

        public void StartChangeScene(MonoBehaviour mono, SceneReference sceneReference, bool bUnloadPrevScene = true)
        {
            if (_loadSceneMode == LoadSceneMode.Additive)
            {
                mono.StartCoroutine(ChangeSceneAsyncAdditive(sceneReference, bUnloadPrevScene));
            }
            else
            {
                mono.StartCoroutine(ChangeSceneAsyncSingle(sceneReference));
            }
        }
        

        private IEnumerator ChangeSceneAsyncSingle(SceneReference sceneReference)
        {
            Scene prevScene = SceneManager.GetActiveScene();
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneReference.ScenePath);
            yield return LoadSceneAsync(buildIndex, prevScene, _loadSceneMode);
        }

        private IEnumerator ChangeSceneAsyncAdditive(SceneReference sceneReference, bool bUnloadPrevScene)
        {
            Scene prevScene = SceneManager.GetActiveScene();
            if (prevScene.IsValid())
            {
                int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneReference.ScenePath);
                yield return LoadSceneAsync(buildIndex, prevScene, _loadSceneMode);
                yield return null;
                
                Scene mainScene = SceneManager.GetSceneByBuildIndex(buildIndex);
                while (!mainScene.isLoaded)
                {
                    yield return null;
                    // 대기하는 동안 씬 객체의 상태가 업데이트되므로 다시 가져옵니다.
                    mainScene = SceneManager.GetSceneByBuildIndex(buildIndex); 
                }
                SceneManager.SetActiveScene(mainScene);
                
                if (bUnloadPrevScene)
                {
                    GameObject[] objsToUnload = prevScene.GetRootGameObjects();
                    for (int i = 0; i < objsToUnload.Length; i++)
                    {
                        objsToUnload[i].SetActive(false);
                    }
                    yield return SceneManager.UnloadSceneAsync(prevScene);
                }

            }
        }
        
        private IEnumerator LoadSceneAsync(int buildIndex, Scene prevScene, LoadSceneMode loadSceneMode)
        {
            _asyncOperation = SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);
            if (_asyncOperation == null)
            {
                yield break;
            }
            _asyncOperation.allowSceneActivation = false;
            while (!_asyncOperation.isDone)
            {
                float curProgress = _asyncOperation.progress;
                _curProgressPct.Value = Mathf.Clamp((curProgress / 0.9f) * 100f, 0f, 100f);
                if (curProgress >= 0.9f)
                {
                    yield return new WaitForSeconds(_loadDelayTime);
                    break;
                }
                yield return null;
            }
            Debug.Log("Scene Loaded");
            
            _asyncOperation.allowSceneActivation = true;
            _curProgressPct.Value = 100;
            OnLoadCompleted?.Invoke(prevScene);
        }
        
        public void SetLoadSceneMode(LoadSceneMode loadSceneMode)
        {
            _loadSceneMode = loadSceneMode;
        }
        
        public void SetLoadDelayTime(float delayTime)
        {
            _loadDelayTime = delayTime;
        }
        
        public ReactiveProperty<float> CurrentLoadProgressPct => _curProgressPct;
    }
}
