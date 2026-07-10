using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KoiAI.Utilities
{
    public static class AsyncSceneLoader
    {
        public static void LoadSceneAsync(this MonoBehaviour mono, SceneReference sceneReference)
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneReference.ScenePath);
            mono.StartCoroutine(LoadSceneAsync(buildIndex));
        }
        
        private static IEnumerator LoadSceneAsync(int buildIndex)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
            if (asyncLoad == null)
            {
                yield break;
            }
            while (!asyncLoad.isDone)
            {
                
            }
            yield break;
        }
    }
}
