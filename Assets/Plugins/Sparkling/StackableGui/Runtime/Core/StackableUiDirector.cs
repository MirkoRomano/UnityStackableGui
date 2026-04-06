using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkling.StackableGui
{
    public class StackableUiDirector : IStackableUiDirector
    {
        private readonly Dictionary<CanvasType, StackableUiCanvas> m_guiCanvases = new Dictionary<CanvasType, StackableUiCanvas>();
        private readonly CanvasType[] m_canvasTypes = (CanvasType[])Enum.GetValues(typeof(CanvasType));

        private IUiAssetLoader m_assetLoader;
        private CanvasSetting m_canvasSetting;

        private StackableUiCanvas m_parentCanvas;
        private Rect m_safeArea = Rect.zero;
        private bool m_initialized = false;

        /// <summary>True after <see cref="GenerateCanvases"/> has completed successfully.</summary>
        public bool Initialized => m_initialized;

        /// <summary>True if the current safe area differs from the full screen rect.</summary>
        public bool HasSafeArea
        {
            get
            {
                bool hasOffsetFromOrigin = m_safeArea.xMin > 0f || m_safeArea.yMin > 0f;
                bool hasOffsetFromScreenEnd = m_safeArea.xMax < Screen.width || m_safeArea.yMax < Screen.height;
                return hasOffsetFromOrigin || hasOffsetFromScreenEnd;
            }
        }

        /// <summary>Creates a new director with the given canvas settings and asset loader.</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assetLoader"/> is null.</exception>
        public StackableUiDirector(CanvasSetting canvasSetting, IUiAssetLoader assetLoader)
        {
            if (assetLoader == null) 
            {
                throw new ArgumentNullException(nameof(assetLoader));
            }

            m_canvasSetting = canvasSetting;
            m_assetLoader = assetLoader;
        }

        /// <summary>Creates the parent canvas and one sub-canvas per <see cref="CanvasType"/> under <paramref name="parent"/>.</summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="CanvasSetting.OrderStep"/> is not greater than zero.</exception>
        public void GenerateCanvases(Transform parent)
        {
            if (m_initialized)
            {
                Debug.LogWarning("Already initialized");
                return;
            }

            if (m_canvasSetting.OrderStep <= 0)
            {
                throw new InvalidOperationException($"{nameof(CanvasSetting.OrderStep)} must be greater than 0.");
            }

            m_parentCanvas = CreateCanvas("StackableCanvases", m_canvasSetting.BaseOrder, parent, false);
            m_safeArea = Screen.safeArea;

            foreach (CanvasType type in m_canvasTypes)
            {
                int order = m_canvasSetting.BaseOrder + (m_canvasSetting.OrderStep * (int)type);
                StackableUiCanvas canvas = CreateCanvas(type.ToString(), order, m_parentCanvas.transform, true);

                ApplySafeAreaToCanvas(canvas);
                m_guiCanvases.TryAdd(type, canvas);

                Debug.Log($"Created canvas: {type}");
            }

            m_initialized = true;
        }

        /// <summary>Refreshes the safe area anchors on all canvases if the safe area has changed since last call.</summary>
        public void ApplySafeArea()
        {
            Rect currentSafeArea = Screen.safeArea;

            if (m_safeArea == currentSafeArea)
            {
                return;
            }

            m_safeArea = currentSafeArea;

            foreach (KeyValuePair<CanvasType, StackableUiCanvas> canvas in m_guiCanvases)
            {
                ApplySafeAreaToCanvas(canvas.Value);
            }
        }

        /// <summary>Synchronously pushes an element onto the specified canvas stack.</summary>
        public void PushUiElementIntoCanvas(IStackableUIElement element,
                                            CanvasType type,
                                            StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                            InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PushUiElement(element, mode);
            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Asynchronously pushes an element onto the specified canvas stack, invoking <paramref name="callback"/> on completion.</summary>
        public void PushUiElementIntoCanvasCallback(IStackableUIElement element,
                                                    CanvasType type,
                                                    StackVisibilityMode mode,
                                                    InputBlockingMode blockingMode = InputBlockingMode.BlockNone,
                                                    Action callback = null)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PushUiElementCallback(element, mode, () =>
            {
                UpdateInputBlocking(blockingMode);
                callback?.Invoke();
            });
        }

        /// <summary>Pops the top element from the specified canvas stack.</summary>
        public void PopUiElementFromCanvas(CanvasType type,
                                           StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                           InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PopUiElement(mode);
            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Pops the top element from the specified canvas stack only if it is of type <typeparamref name="T"/>.</summary>
        public void PopUiElementFromCanvasIfMatch<T>(CanvasType type,
                                                     StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                                     InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PopUiElement<T>(mode);
            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Pops the top element from the specified canvas stack, invoking <paramref name="callback"/> on completion.</summary>
        public void PopUiElementFromCanvasCallback(CanvasType type,
                                                   Action callback,
                                                   StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                                   InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PopUiElementCallback(() =>
            {
                UpdateInputBlocking(blockingMode);
                callback?.Invoke();
            }, mode);
        }

        /// <summary>Pops the top element from the specified canvas stack only if it is of type <typeparamref name="T"/>, invoking <paramref name="callback"/> on completion.</summary>
        public void PopUiElementFromCanvasCallbackIfMatch<T>(CanvasType type,
                                                             Action callback,
                                                             StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                                             InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.PopUiElementCallback<T>(() =>
            {
                UpdateInputBlocking(blockingMode);
                callback?.Invoke();
            }, mode);
        }

        /// <summary>Synchronously inserts an element at the given index in the specified canvas stack.</summary>
        public void InsertUiElementInCanvas(IStackableUIElement element,
                                            int index,
                                            CanvasType type,
                                            StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.InsertUiElement(element, index, mode);
        }

        /// <summary>Removes a specific element from any position in the specified canvas stack.</summary>
        public void RemoveUiElementFromCanvas(IStackableUIElement element,
                                              CanvasType type,
                                              StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                              InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.RemoveUiElement(element, mode);
            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Removes all elements from the specified canvas stack.</summary>
        public void ClearCanvas(CanvasType type, InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.ClearCanvas();
            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Removes all elements from every canvas stack.</summary>
        public void ClearAllCanvases(InputBlockingMode blockingMode = InputBlockingMode.BlockNone)
        {
            if (!m_initialized)
            {
                Debug.LogWarning($"{nameof(StackableUiDirector)} not initialized yet");
                return;
            }

            foreach (KeyValuePair<CanvasType, StackableUiCanvas> canvas in m_guiCanvases)
            {
                if (canvas.Value == null)
                {
                    continue;
                }

                canvas.Value.ClearCanvas();
            }

            UpdateInputBlocking(blockingMode);
        }

        /// <summary>Returns true if the specified canvas stack contains at least one element.</summary>
        public bool AnyElementInCanvas(CanvasType type)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return false;
            }

            return canvas.AnyElement();
        }

        /// <summary>Returns the first element of type <typeparamref name="T"/> found in the specified canvas stack.</summary>
        public bool TryFindElementInCanvas<T>(out T element, CanvasType type) where T : IStackableUIElement
        {
            element = default;

            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return false;
            }

            return canvas.TryFindElement<T>(out element);
        }

        /// <summary>Returns all elements of type <typeparamref name="T"/> found in the specified canvas stack.</summary>
        public bool TryFindElementsInCanvas<T>(out T[] elements, CanvasType type) where T : IStackableUIElement
        {
            elements = default;

            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return false;
            }

            return canvas.TryFindElements<T>(out elements);
        }

        /// <summary>Applies new canvas scaler settings to the parent canvas.</summary>
        public void ChangeCanvasResolution(CanvasSetting settings)
        {
            if (!m_initialized)
            {
                Debug.LogWarning($"{nameof(StackableUiDirector)} not initialized yet");
                return;
            }

            if (m_parentCanvas.UiScaler == null)
            {
                Debug.LogWarning($"Parent canvas has no CanvasScaler. Resolution change skipped.");
                return;
            }

            ApplySettings(settings, m_parentCanvas.UiScaler);
            m_canvasSetting = settings;
        }

        /// <summary>Plays a screen shake on the specified canvas.</summary>
        public void ShakeCanvas(CanvasType type, float duration, float magnitude, float frequency)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.Shake(duration, magnitude, frequency);
        }

        /// <summary>Plays a screen shake on all canvases simultaneously.</summary>
        public void ShakeAllCanvases(float duration, float magnitude, float frequency)
        {
            if (!m_initialized)
            {
                Debug.LogWarning($"{nameof(StackableUiDirector)} not initialized yet");
                return;
            }

            foreach (KeyValuePair<CanvasType, StackableUiCanvas> canvas in m_guiCanvases)
            {
                if (canvas.Value == null)
                {
                    continue;
                }

                canvas.Value.Shake(duration, magnitude, frequency);
            }
        }

        /// <summary>Clears and destroys the specified canvas, removing it from the director.</summary>
        public void DestroyCanvas(CanvasType type)
        {
            if (!TryGetCanvas(type, out StackableUiCanvas canvas))
            {
                return;
            }

            canvas.ClearCanvas();

            if (Application.isPlaying)
            {
                GameObject.Destroy(canvas.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(canvas.gameObject);
            }

            m_guiCanvases.Remove(type);
        }

        private bool TryGetCanvas(CanvasType type, out StackableUiCanvas canvas)
        {
            canvas = null;

            if (!m_initialized)
            {
                Debug.LogWarning($"{nameof(StackableUiDirector)} not initialized yet");
                return false;
            }

            if (!m_guiCanvases.TryGetValue(type, out canvas))
            {
                Debug.LogWarning($"No canvas of type {type} was found");
                return false;
            }

            if (canvas == null)
            {
                Debug.LogWarning($"Canvas of type {type} exists but has been destroyed");
                return false;
            }

            return true;
        }

        private void UpdateInputBlocking(InputBlockingMode mode)
        {
            if (mode == InputBlockingMode.BlockNone)
            {
                foreach (KeyValuePair<CanvasType, StackableUiCanvas> pair in m_guiCanvases)
                {
                    pair.Value?.SetInputBlocking(InputBlockingMode.BlockNone, isTopCanvas: true);
                }

                return;
            }

            CanvasType? topCanvasType = null;
            foreach (CanvasType type in m_canvasTypes)
            {
                if (m_guiCanvases.TryGetValue(type, out StackableUiCanvas c) && c != null && c.AnyElement())
                {
                    topCanvasType = type;
                }
            }

            foreach (KeyValuePair<CanvasType, StackableUiCanvas> pair in m_guiCanvases)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                bool isTop = pair.Key == topCanvasType;
                pair.Value.SetInputBlocking(mode, isTop);
            }
        }

        private StackableUiCanvas CreateCanvas(string name, int order, Transform parent, bool isSubCanvas)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, worldPositionStays: false);

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = m_canvasSetting.CanvasRenderMode;
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            if (isSubCanvas)
            {
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                ApplySettings(m_canvasSetting, go.AddComponent<CanvasScaler>());
            }

            go.AddComponent<GraphicRaycaster>();

            StackableUiCanvas stackableCanvas = go.AddComponent<StackableUiCanvas>();
            stackableCanvas.Initialize(m_assetLoader);
            return stackableCanvas;
        }

        private void ApplySettings(CanvasSetting settings, CanvasScaler scaler)
        {
            scaler.uiScaleMode = settings.ScaleMode;
            scaler.referenceResolution = settings.ReferenceResolution;
            scaler.screenMatchMode = settings.MatchMode;
            scaler.matchWidthOrHeight = Mathf.Clamp01(settings.Match);
            scaler.referencePixelsPerUnit = settings.ReferencePixelsPerUnit;
        }

        private void ApplySafeAreaToCanvas(StackableUiCanvas stackableCanvas)
        {
            if (stackableCanvas == null)
            {
                Debug.LogWarning("Cannot apply a safe area to a null canvas");
                return;
            }

            Canvas rootCanvas = stackableCanvas.UiCanvas;

            if (rootCanvas == null)
            {
                Debug.LogWarning($"No canvas found in {stackableCanvas.name}");
                return;
            }

            Vector2 anchorMin;
            Vector2 anchorMax;

            switch (rootCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    {
                        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
                        anchorMin = new Vector2(m_safeArea.xMin / screenSize.x, m_safeArea.yMin / screenSize.y);
                        anchorMax = new Vector2(m_safeArea.xMax / screenSize.x, m_safeArea.yMax / screenSize.y);
                        break;
                    }
                case RenderMode.ScreenSpaceCamera:
                    {
                        if (rootCanvas.worldCamera == null)
                        {
                            Debug.LogWarning($"Canvas '{rootCanvas.name}' is ScreenSpaceCamera but has no camera assigned.");
                            return;
                        }

                        Camera cam = rootCanvas.worldCamera;
                        Vector3 minViewport = cam.ScreenToViewportPoint(new Vector3(m_safeArea.xMin, m_safeArea.yMin));
                        Vector3 maxViewport = cam.ScreenToViewportPoint(new Vector3(m_safeArea.xMax, m_safeArea.yMax));
                        anchorMin = new Vector2(minViewport.x, minViewport.y);
                        anchorMax = new Vector2(maxViewport.x, maxViewport.y);
                        break;
                    }
                default:
                    {
                        Debug.Log($"ApplySafeArea: Canvas '{rootCanvas.name}' is WorldSpace - safe area not applied.");
                        return;
                    }
            }

            RectTransform rt = stackableCanvas.UiRectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }
}