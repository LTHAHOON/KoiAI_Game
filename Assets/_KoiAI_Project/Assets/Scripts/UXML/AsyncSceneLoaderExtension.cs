using UnityEngine.SceneManagement;

namespace KoiAI.Utilities
{
    public static class AsyncSceneLoaderExtension
    {
        public static IAsyncSceneLoadHandler SetLoadSceneMode(this IAsyncSceneLoadHandler asyncSceneLoader, LoadSceneMode loadSceneMode)
        {
            AsyncSceneLoader loader = (AsyncSceneLoader)asyncSceneLoader;
            loader.SetLoadSceneMode(loadSceneMode);
            return asyncSceneLoader;
        }
    }
}
