using System;
using System.Collections;
using UnityEngine;

namespace ToolBox.Utils
{
    public static class CoroutineUtils
    {
        public static IEnumerator LoadFromResources<T>(string path, Action<T> onCompleted) where T : UnityEngine.Object
        {
            ResourceRequest handle = Resources.LoadAsync<T>(path);

            yield return handle;

            onCompleted?.Invoke(handle.asset as T);
        }
    }
}
