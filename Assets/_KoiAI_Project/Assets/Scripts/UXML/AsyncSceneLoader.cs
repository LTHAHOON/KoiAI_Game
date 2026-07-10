using R3;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KoiAI.Utilities
{
    //확장클래스와 함수명 겹치는 문제와 은닉화를 위해 인터페이스 활용 
    public interface IAsyncSceneLoadHandler 
    {
        /// <summary>
        /// 비동기 로드 실행
        /// </summary>
        public void StartLoadAsync(MonoBehaviour mono, SceneReference sceneReference);

        /// <summary>
        /// 진행률(0~100%)
        /// </summary>
        public ReactiveProperty<float> CurrentProgressPct { get; }
    }

    public class AsyncSceneLoader : IAsyncSceneLoadHandler
    {
        private ReactiveProperty<float> _curProgressPct = new(0);
        private LoadSceneMode _loadSceneMode = LoadSceneMode.Single;

        public static IAsyncSceneLoadHandler CreateAsyncSceneLoadHandler()
        {
            IAsyncSceneLoadHandler loadHandler = new AsyncSceneLoader();
            return loadHandler;
        }

        public void StartLoadAsync(MonoBehaviour mono, SceneReference sceneReference)
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneReference.ScenePath);
            mono.StartCoroutine(LoadSceneAsync(buildIndex, _loadSceneMode));
        }

        private IEnumerator LoadSceneAsync(int buildIndex, LoadSceneMode loadSceneMode)
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.UnloadSceneAsync(scene);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);
            if (asyncOperation == null)
            {
                yield break;
            }
            asyncOperation.allowSceneActivation = false;
            while (!asyncOperation.isDone)
            {
                float curProgress = asyncOperation.progress;
                _curProgressPct.Value = (curProgress / 0.9f) * 100;
                if (curProgress >= 0.9f)
                {
                    _curProgressPct.Value = 100;
                   yield return new WaitForSeconds(1f);
                    asyncOperation.allowSceneActivation = true;
                }
                yield return null;
            }

        }


        public void SetLoadSceneMode(LoadSceneMode loadSceneMode)
        {
            _loadSceneMode = loadSceneMode;
        }


        public ReactiveProperty<float> CurrentProgressPct => _curProgressPct;
    }
}
