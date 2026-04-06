using System;
using UnityEngine;

namespace Sparkling.StackableGui
{
    public interface IUiAssetLoader
    {
        GameObject LoadAsset(string path);
        void LoadAssetAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null);
        void ReleasePrefab(GameObject prefab);
    }
}