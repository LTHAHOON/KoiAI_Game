using System;
using System.Threading;
using R3;
using UnityEngine.SceneManagement;

namespace KoiAI.Utilities
{
    public static class AsyncSceneChangeExtension
    {
        public static IAsyncSceneChangeHandler AssignOnLoadCompleted(this IAsyncSceneChangeHandler asyncSceneChanger, Action<Scene> onLoadCompleted)
        {
            AsyncSceneChanger changer = (AsyncSceneChanger)asyncSceneChanger;
            if (onLoadCompleted != null)
            {
                changer.OnLoadCompleted += onLoadCompleted;
            }
            return asyncSceneChanger;
        }

        public static IAsyncSceneChangeHandler SetLoadDelayTime(this IAsyncSceneChangeHandler asyncSceneChanger, float delayTime)
        {
            AsyncSceneChanger changer = (AsyncSceneChanger)asyncSceneChanger;
            changer.SetLoadDelayTime(delayTime);
            return asyncSceneChanger;
        }
        
        public static IAsyncSceneChangeHandler SetLoadSceneMode(this IAsyncSceneChangeHandler asyncSceneChanger, LoadSceneMode loadSceneMode)
        {
            AsyncSceneChanger changer = (AsyncSceneChanger)asyncSceneChanger;
            changer.SetLoadSceneMode(loadSceneMode);
            return asyncSceneChanger;
        }
        
        public static IAsyncSceneChangeHandler SetProgressSubscribe(this IAsyncSceneChangeHandler asyncSceneChanger, Action<float> progressAction, CancellationToken cancellationToken)
        {
            asyncSceneChanger.CurrentLoadProgressPct.Subscribe(progressAction).RegisterTo(cancellationToken);
            return asyncSceneChanger;
        }
    }
}
