using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkling.StackableGui
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    public class StackableUiCanvas : MonoBehaviour
    {
        public RectTransform UiRectTransform => m_rectTransform;
        public Canvas UiCanvas => m_canvas;
        public CanvasScaler UiScaler => m_scaler;
        public GraphicRaycaster UiRaycaster => m_raycaster;

        /// <summary>Fired whenever the stack changes (push, pop, or clear).</summary>
        public event Action<StackChangedEventArgs> OnStackChanged;

        private readonly LinkedList<IStackableUIElement> m_guiElements = new LinkedList<IStackableUIElement>();

        private IUiAssetLoader m_assetLoader;
        private Canvas m_canvas;
        private CanvasScaler m_scaler;
        private RectTransform m_rectTransform;
        private GraphicRaycaster m_raycaster;

        private Coroutine m_shakeCoroutine;

        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_canvas = GetComponent<Canvas>();
            m_raycaster = GetComponent<GraphicRaycaster>();

            // If this is a sub-canvas, take the scaler directly from the parent
            if (!TryGetComponent<CanvasScaler>(out m_scaler))
            {
                m_scaler = GetComponentInParent<CanvasScaler>();
            }
        }

        /// <summary>Initializes the canvas with the asset loader. Must be called before any push or pop.</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assetLoader"/> is null.</exception>
        public void Initialize(IUiAssetLoader assetLoader)
        {
            if (assetLoader == null)
            {
                throw new ArgumentNullException(nameof(assetLoader));
            }

            m_assetLoader = assetLoader;
        }

        private void OnDestroy()
        {
            if (m_shakeCoroutine != null)
            {
                StopCoroutine(m_shakeCoroutine);
                m_shakeCoroutine = null;
            }

            OnStackChanged = null;
            ClearCanvas();
        }

        /// <summary>Synchronously loads and pushes an element onto the stack.</summary>
        public void PushUiElement(IStackableUIElement element, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (IsAlreadyInStack(element))
            {
                Debug.LogWarning($"Element '{element.Path}' is already in the stack of {gameObject.name}. Skipping.");
                return;
            }

            GameObject asset = m_assetLoader.LoadAsset(element.Path);

            if (asset == null)
            {
                Debug.LogWarning($"Asset for '{element.Path}' is null. Skipping insertion in {gameObject.name}.");
                return;
            }

            int oldStackSize = m_guiElements.Count;
            element.Initialize(asset);
            m_guiElements.AddLast(element);

            ApplyVisibilityMode(mode);
            element.OnPushedIntoStack();

            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Pushed, element, oldStackSize, m_guiElements.Count));
        }

        /// <summary>Asynchronously loads and pushes an element onto the stack, invoking <paramref name="callback"/> on completion.</summary>
        public void PushUiElementCallback(IStackableUIElement element, StackVisibilityMode mode, Action callback = null)
        {
            if (IsAlreadyInStack(element))
            {
                Debug.LogWarning($"Element '{element.Path}' is already in the stack of {gameObject.name}. Skipping.");
                return;
            }

            int oldStackSize = m_guiElements.Count;
            m_assetLoader.LoadAssetAsync(element.Path, OnPrefabLoaded, OnError);

            void OnPrefabLoaded(GameObject prefab)
            {
                element.Initialize(prefab);
                m_guiElements.AddLast(element);

                ApplyVisibilityMode(mode);
                element.OnPushedIntoStack();

                OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Pushed, element, oldStackSize, m_guiElements.Count));
                callback?.Invoke();
            }

            void OnError(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>Removes the top element from the stack.</summary>
        public void PopUiElement(StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (m_guiElements.Count <= 0)
            {
                Debug.LogWarning($"No elements to pop from {gameObject.name} canvas");
                return;
            }

            int oldStackSize = m_guiElements.Count;
            IStackableUIElement elementToRemove = m_guiElements.Last!.Value;
            m_guiElements.RemoveLast();

            elementToRemove.OnPoppedFromStack();
            m_assetLoader.ReleasePrefab(elementToRemove.Prefab);

            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Popped, elementToRemove, oldStackSize, m_guiElements.Count));

            if (m_guiElements.Count <= 0)
            {
                return;
            }

            ApplyVisibilityMode(mode);
        }

        /// <summary>Pops the top element only if it is of type <typeparamref name="T"/>.</summary>
        public void PopUiElement<T>(StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (m_guiElements.Count <= 0)
            {
                Debug.LogWarning($"No elements to pop from {gameObject.name} canvas");
                return;
            }

            if (m_guiElements.Last!.Value is not T)
            {
                Debug.LogWarning($"The top element is not of type {typeof(T)}");
                return;
            }

            PopUiElement(mode);
        }

        /// <summary>Removes the top element from the stack, invoking <paramref name="callback"/> on completion.</summary>
        public void PopUiElementCallback(Action callback, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (m_guiElements.Count <= 0)
            {
                Debug.LogWarning($"No elements to pop from {gameObject.name} canvas");
                return;
            }

            int oldStackSize = m_guiElements.Count;
            IStackableUIElement elementToRemove = m_guiElements.Last!.Value;
            m_guiElements.RemoveLast();

            elementToRemove.OnPoppedFromStack();
            m_assetLoader.ReleasePrefab(elementToRemove.Prefab);

            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Popped, elementToRemove, oldStackSize, m_guiElements.Count));

            if (m_guiElements.Count <= 0)
            {
                return;
            }

            ApplyVisibilityMode(mode);
            callback?.Invoke();
        }

        /// <summary>Pops the top element only if it is of type <typeparamref name="T"/>, invoking <paramref name="callback"/> on completion.</summary>
        public void PopUiElementCallback<T>(Action callback, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (m_guiElements.Count <= 0)
            {
                Debug.LogWarning($"No elements to pop from {gameObject.name} canvas");
                return;
            }

            if (m_guiElements.Last!.Value is not T)
            {
                Debug.LogWarning($"The top element is not of type {typeof(T)}");
                return;
            }

            PopUiElementCallback(callback, mode);
        }

        /// <summary>Synchronously loads and inserts an element at the given stack index.</summary>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is out of range.</exception>
        public void InsertUiElement(IStackableUIElement element, int index, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            if (IsAlreadyInStack(element))
            {
                Debug.LogWarning($"Element '{element.Path}' is already in the stack of {gameObject.name}. Skipping.");
                return;
            }

            if (index < 0 || index > m_guiElements.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index {index} for UI element insertion in {gameObject.name}. Stack size: {m_guiElements.Count}");
            }

            GameObject asset = m_assetLoader.LoadAsset(element.Path);

            if (asset == null)
            {
                Debug.LogWarning($"Asset for '{element.Path}' is null. Skipping insertion in {gameObject.name}.");
                return;
            }

            element.Initialize(asset);

            LinkedListNode<IStackableUIElement> nodeAtIndex = GetNodeAt(index);

            int oldStackSize = m_guiElements.Count;
            if (nodeAtIndex == null)
            {
                m_guiElements.AddLast(element);
            }
            else
            {
                m_guiElements.AddBefore(nodeAtIndex, element);
            }

            element.OnPushedIntoStack();
            ReorderUiElements();
            ApplyVisibilityMode(mode);

            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Pushed, element, oldStackSize, m_guiElements.Count));
        }

        /// <summary>
        /// Asynchronously loads and inserts an element at the given stack index, invoking <paramref name="callback"/> on completion.
        /// Aborts if the stack changes during the async load.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is out of range.</exception>
        public void InsertUiElementCallback(IStackableUIElement element, int index, Action callback = null)
        {
            if (IsAlreadyInStack(element))
            {
                Debug.LogWarning($"Element '{element.Path}' is already in the stack of {gameObject.name}. Skipping.");
                return;
            }

            if (index < 0 || index > m_guiElements.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index {index} for UI element insertion in {gameObject.name}. Stack size: {m_guiElements.Count}");
            }

            int expectedCount = m_guiElements.Count;
            m_assetLoader.LoadAssetAsync(element.Path, OnPrefabLoaded, OnError);

            void OnPrefabLoaded(GameObject prefab)
            {
                if (m_guiElements.Count != expectedCount)
                {
                    Debug.LogWarning($"Stack changed during async load in {gameObject.name}. Aborting insert at index {index}.");
                    m_assetLoader.ReleasePrefab(prefab);
                    return;
                }

                LinkedListNode<IStackableUIElement> nodeAtIndex = GetNodeAt(index);

                if (nodeAtIndex == null && index < m_guiElements.Count)
                {
                    Debug.LogWarning($"Index {index} is no longer valid after async load in {gameObject.name}. Inserting at top.");
                }

                element.Initialize(prefab);

                int oldStackSize = m_guiElements.Count;
                if (nodeAtIndex == null)
                {
                    m_guiElements.AddLast(element);
                }
                else
                {
                    m_guiElements.AddBefore(nodeAtIndex, element);
                }

                element.OnPushedIntoStack();
                ReorderUiElements();

                OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Pushed, element, oldStackSize, m_guiElements.Count));
                callback?.Invoke();
            }

            void OnError(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>Removes a specific element from any position in the stack. Returns false if the element is not found.</summary>
        public bool RemoveUiElement(IStackableUIElement element, StackVisibilityMode mode = StackVisibilityMode.AllVisible)
        {
            LinkedListNode<IStackableUIElement> node = m_guiElements.Find(element);

            if (node == null)
            {
                Debug.LogWarning($"Element not found in {gameObject.name}");
                return false;
            }

            int oldStackSize = m_guiElements.Count;
            bool wasTop = node == m_guiElements.Last;
            m_guiElements.Remove(node);

            element.OnPoppedFromStack();
            m_assetLoader.ReleasePrefab(element.Prefab);

            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Popped, element, oldStackSize, m_guiElements.Count));

            if (m_guiElements.Count <= 0)
            {
                return true;
            }

            if (wasTop)
            {
                ApplyVisibilityMode(mode);
            }

            ReorderUiElements();
            return true;
        }

        /// <summary>Removes all elements from the stack and releases their prefabs.</summary>
        public void ClearCanvas()
        {
            if (m_guiElements.Count <= 0)
            {
                return;
            }

            int oldStackSize = m_guiElements.Count;
            LinkedListNode<IStackableUIElement> node = m_guiElements.Last;

            while (node != null)
            {
                IStackableUIElement element = node.Value;
                node = node.Previous;

                if (element == null)
                {
                    continue;
                }

                element?.OnPoppedFromStack();
                m_assetLoader.ReleasePrefab(element.Prefab);
            }

            m_guiElements.Clear();
            OnStackChanged?.Invoke(new StackChangedEventArgs(StackChangeType.Cleared, null, oldStackSize, m_guiElements.Count));
        }

        /// <summary>Enables or disables the raycaster based on the blocking mode and whether this is the topmost canvas.</summary>
        public void SetInputBlocking(InputBlockingMode mode, bool isTopCanvas)
        {
            if (m_raycaster == null)
            {
                return;
            }

            m_raycaster.enabled = mode switch
            {
                InputBlockingMode.BlockNone => true,
                InputBlockingMode.BlockAll => false,
                InputBlockingMode.BlockBelowTop => isTopCanvas,
                _ => true
            };
        }

        /// <summary>Reorders the sibling indices of all element instances to match the stack order.</summary>
        public void ReorderUiElements()
        {
            if (m_rectTransform == null)
            {
                Debug.LogWarning($"RectTransform not initialized on {gameObject.name}");
                return;
            }

            if (m_guiElements.Count == 0)
            {
                return;
            }

            int siblingIndex = 0;
            foreach (IStackableUIElement element in m_guiElements)
            {
                if (element?.Instance != null)
                {
                    element.Instance.transform.SetSiblingIndex(siblingIndex++);
                }
            }
        }

        /// <summary>Returns true if the stack contains at least one element.</summary>
        public bool AnyElement()
        {
            return m_guiElements.Count > 0;
        }

        /// <summary>Returns the first element of type <typeparamref name="T"/> found in the stack.</summary>
        public bool TryFindElement<T>(out T element) where T : IStackableUIElement
        {
            foreach (IStackableUIElement e in m_guiElements)
            {
                if (e is T match)
                {
                    element = match;
                    return true;
                }
            }

            element = default;
            return false;
        }

        /// <summary>Returns all elements of type <typeparamref name="T"/> found in the stack.</summary>
        public bool TryFindElements<T>(out T[] elements) where T : IStackableUIElement
        {
            elements = m_guiElements.OfType<T>().ToArray();
            return elements.Length > 0;
        }

        /// <summary>Returns true if the first element of type <typeparamref name="T"/> in the stack is currently animating.</summary>
        public bool IsElementAnimating<T>() where T : IStackableUIElement
        {
            return TryFindElement<T>(out T element) && element.IsAnimating;
        }

        /// <summary>Plays a Perlin-noise screen shake on this canvas. Ignored if a shake is already in progress.</summary>
        public void Shake(float duration, float magnitude, float frequency)
        {
            if (m_shakeCoroutine != null)
            {
                return;
            }

            m_shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude, frequency));
        }

        private bool IsAlreadyInStack(IStackableUIElement element)
        {
            return m_guiElements.Contains(element);
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude, float frequency)
        {
            float elapsed = 0f;
            float seed = UnityEngine.Random.value * 100f;
            RectTransform canvas = (RectTransform)m_canvas.transform;
            Vector3 originalPos = canvas.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float x = (Mathf.PerlinNoise(seed, elapsed * frequency) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(seed + 1f, elapsed * frequency) - 0.5f) * 2f;
                float damper = 1f - Mathf.Clamp01(progress);
                canvas.localPosition = originalPos + new Vector3(x, y, 0f) * magnitude * damper;
                yield return null;
            }

            canvas.localPosition = originalPos;
            m_shakeCoroutine = null;
        }

        private LinkedListNode<IStackableUIElement> GetNodeAt(int index)
        {
            int i = 0;
            for (LinkedListNode<IStackableUIElement> node = m_guiElements.First; node != null; node = node.Next)
            {
                if (i == index)
                {
                    return node;
                }

                i++;
            }

            return null;
        }

        private void ApplyVisibilityMode(StackVisibilityMode visibilityMode)
        {
            if (m_guiElements.Count == 0)
            {
                return;
            }

            switch (visibilityMode)
            {
                case StackVisibilityMode.TopOnly:
                    {
                        LinkedListNode<IStackableUIElement> node = m_guiElements.Last;
                        bool isTop = true;
                        while (node != null)
                        {
                            node.Value.SetActive(isTop);
                            isTop = false;
                            node = node.Previous;
                        }
                    }
                    break;
                case StackVisibilityMode.AllVisible:
                    {
                        foreach (IStackableUIElement e in m_guiElements)
                        {
                            e.SetActive(true);
                        }
                    }
                    break;
            }
        }
    }
}