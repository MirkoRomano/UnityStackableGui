using System;
using UnityEngine;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableGuiDirector : MonoBehaviour, IStackableUiDirector
    {
        public static SampleStackableGuiDirector Instance;

        public static bool Active => Instance != null && Instance.gameObject.activeSelf;

        [SerializeField]
        private CanvasSetting m_settings;

        private StackableUiDirector m_director;

        public bool Initialized => m_director.Initialized;
        public bool HasSafeArea => m_director.HasSafeArea;

        private void Awake()
        {
            Instance = this;
            m_director = new StackableUiDirector(m_settings, new SampleResourcesLoader());
        }

        private void Start()
        {
            m_director.GenerateCanvases(transform);
        }

        private void OnDestroy()
        {
            foreach (CanvasType canvas in Enum.GetValues(typeof(CanvasType)))
            {
                m_director.DestroyCanvas(canvas);
            }
        }

        public void GenerateCanvases(Transform parent) => m_director.GenerateCanvases(parent);
        public void ApplySafeArea() => m_director.ApplySafeArea();

        public void PushUiElementIntoCanvas(IStackableUIElement element, CanvasType type, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.PushUiElementIntoCanvas(element, type, mode, blockingMode);

        public void PushUiElementIntoCanvasCallback(IStackableUIElement element, CanvasType type, StackVisibilityMode mode, InputBlockingMode blockingMode = InputBlockingMode.BlockNone, Action callback = null)
            => m_director.PushUiElementIntoCanvasCallback(element, type, mode, blockingMode, callback);

        public void PopUiElementFromCanvas(CanvasType type, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.PopUiElementFromCanvas(type, mode, blockingMode);

        public void PopUiElementFromCanvasIfMatch<T>(CanvasType type, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.PopUiElementFromCanvasIfMatch<T>(type, mode, blockingMode);

        public void PopUiElementFromCanvasCallback(CanvasType type, Action callback, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.PopUiElementFromCanvasCallback(type, callback, mode, blockingMode);

        public void PopUiElementFromCanvasCallbackIfMatch<T>(CanvasType type, Action callback, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.PopUiElementFromCanvasCallbackIfMatch<T>(type, callback, mode, blockingMode);

        public void InsertUiElementInCanvas(IStackableUIElement element, int index, CanvasType type, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
            => m_director.InsertUiElementInCanvas(element, index, type, mode);

        public void RemoveUiElementFromCanvas(IStackableUIElement element, CanvasType type, StackVisibilityMode mode = StackVisibilityMode.AllVisible, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.RemoveUiElementFromCanvas(element, type, mode, blockingMode);

        public void ClearCanvas(CanvasType type, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.ClearCanvas(type, blockingMode);

        public void ClearAllCanvases(InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
            => m_director.ClearAllCanvases(blockingMode);

        public bool AnyElementInCanvas(CanvasType type)
            => m_director.AnyElementInCanvas(type);

        public bool TryFindElementInCanvas<T>(out T element, CanvasType type) where T : IStackableUIElement
            => m_director.TryFindElementInCanvas<T>(out element, type);

        public bool TryFindElementsInCanvas<T>(out T[] elements, CanvasType type) where T : IStackableUIElement
            => m_director.TryFindElementsInCanvas<T>(out elements, type);

        public void ChangeCanvasResolution(CanvasSetting settings)
            => m_director.ChangeCanvasResolution(settings);

        public void ShakeCanvas(CanvasType type, float duration, float magnitude, float frequency)
            => m_director.ShakeCanvas(type, duration, magnitude, frequency);

        public void ShakeAllCanvases(float duration, float magnitude, float frequency)
            => m_director.ShakeAllCanvases(duration, magnitude, frequency);

        public void DestroyCanvas(CanvasType type)
            => m_director.DestroyCanvas(type);

        public class SampleResourcesLoader : IUiAssetLoader
        {
            public GameObject LoadAsset(string path)
            {
                GameObject prefab = Resources.Load<GameObject>(path);

                if (prefab == null)
                {
                    Debug.LogError($"[SampleResourcesLoader] Prefab not found at path: '{path}'");
                    return null;
                }

                return prefab;
            }

            public void LoadAssetAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null)
            {
                try
                {
                    var load = Resources.LoadAsync<GameObject>(path);
                    load.completed += a =>
                    {
                        UnityEngine.Object prefab = load.asset;

                        if (prefab == null)
                        {
                            onError?.Invoke(new Exception($"Prefab not found at path: '{path}'"));
                            return;
                        }

                        onLoaded?.Invoke(prefab as GameObject);
                    };
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
            }

            public void ReleasePrefab(GameObject prefab)
            {

            }
        }
    }
}