# StackableGui

**StackableGui** is a robust, stack-based UI management system for Unity. Designed with architectural decoupling and modularity in mind, it lets developers manage complex UI hierarchies across multiple independent layers with ease.

The system treats each UI section (Background, Gameplay, Popups, System, etc.) as an independent stack, automating rendering order, input blocking, and Safe Area calculations — so you can focus on building features instead of managing canvas bookkeeping.

---

## 🚀 Key Features

- **Stack-Based Navigation** — Manage UI flow naturally with Push, Pop, Insert, and Remove operations. Both synchronous and asynchronous (callback-based) variants are provided for every operation.
- **Multi-Canvas Layering** — Organize UI depth via `CanvasType` (Background → Loading) with automatic sorting order management. Each layer is a fully independent stack.
- **Automatic Safe Area Support** — Native support for mobile notches and screen cutouts. Anchors are dynamically recalculated based on `RenderMode` (ScreenSpaceOverlay and ScreenSpaceCamera are both handled).
- **Abstract Asset Loading** — Integrate Addressables, `Resources`, custom object pooling, or any other loading strategy by implementing a single interface: `IUiAssetLoader`.
- **Visibility Modes** — Choose between `TopOnly` (only the topmost element is active) and `AllVisible` (the full stack stays visible) per operation.
- **Input Blocking** — Three modes: `BlockNone`, `BlockBelowTop`, and `BlockAll`. Applied and updated automatically across all canvases after every operation.
- **Type-Safe Pop / Remove** — `PopUiElementFromCanvasIfMatch<T>` and `RemoveUiElementFromCanvas` let you safely target specific element types without manual bookkeeping.
- **Screen Shake** — Perlin-noise screen shake on a single canvas or all canvases simultaneously, with automatic damping over time.
- **Event-Driven Architecture** — Fully decoupled communication through the `OnStackChanged` event (`StackChangedEventArgs` carries change type, element, old size, and new size).
- **Resolution Hot-Swap** — Change canvas scaler settings at runtime via `ChangeCanvasResolution` without rebuilding the canvas hierarchy.

---

## 🛠 Project Structure

| File | Description |
| :--- | :--- |
| `IStackableUiDirector` | Main API surface for commanding the entire UI system. |
| `StackableUiDirector` | Concrete implementation of `IStackableUiDirector`. Manages all canvases and delegates to them. |
| `StackableUiCanvas` | Per-layer MonoBehaviour that owns the stack, handles sorting, visibility, input, and shake. |
| `IStackableUIElement` | Interface for your UI element controllers (e.g. `MainMenuController`, `PopupController`). |
| `IUiAssetLoader` | Interface for asset loading. Implement to plug in your preferred loading backend. |
| `CanvasSetting` | Serializable struct defining render mode, scale mode, reference resolution, and sorting. Includes `Default`, `Mobile`, and `HighDpi` presets. |
| `GlobalEnums` | Definitions for `CanvasType`, `StackVisibilityMode`, `InputBlockingMode`, and `StackChangeType`. |
| `StackChangedEventArgs` | Readonly event payload emitted by `StackableUiCanvas.OnStackChanged`. |

---

## 📦 Installation

1. Copy the source files into your Unity project under `Assets/`.
2. Implement `IUiAssetLoader` to bridge your preferred loading backend:

```csharp
public class MyResourcesLoader : IUiAssetLoader
{
    public GameObject LoadAsset(string path)
        => Resources.Load<GameObject>(path);

    public void LoadAssetAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null)
    {
        var op = Resources.LoadAsync<GameObject>(path);
        op.completed += _ =>
        {
            if (op.asset is GameObject prefab) onLoaded?.Invoke(prefab);
            else onError?.Invoke(new Exception($"Prefab not found: '{path}'"));
        };
    }

    public void ReleasePrefab(GameObject prefab) { /* no-op for Resources */ }
}
```

3. Create a `MonoBehaviour` that owns a `StackableUiDirector` instance and calls `GenerateCanvases` in `Start`. See `SampleStackableGuiDirector` for a complete reference.

---

## 📋 Quick Start

### Initialization

```csharp
// Construct the director with your loader and a settings preset
var director = new StackableUiDirector(CanvasSetting.Default, new MyResourcesLoader());

// Generate the full canvas hierarchy parented to a Transform in your scene
director.GenerateCanvases(transform);

// Apply Safe Area (call again whenever the screen orientation changes)
director.ApplySafeArea();
```

> **Note:** `CanvasSetting` argument order is `(CanvasSetting, IUiAssetLoader)`.  
> The three built-in presets are `CanvasSetting.Default` (1920×1080), `CanvasSetting.Mobile` (1080×1920, match width), and `CanvasSetting.HighDpi` (2560×1440, 200 ppu).

---

### Implementing an Element

```csharp
public class MainMenuElement : IStackableUIElement
{
    public string Path => "UI/MainMenu"; // path passed to IUiAssetLoader
    public bool IsVisible => Instance != null && Instance.activeSelf;
    public bool IsLoaded  => Instance != null;
    public bool IsAnimating { get; private set; }

    public GameObject Instance { get; private set; }
    public GameObject Prefab   { get; private set; }
    public GameObject Parent   { get; private set; }

    public void Initialize(GameObject prefab, GameObject parent)
    {
        Prefab   = prefab;
        Parent   = parent;
        Instance = Object.Instantiate(prefab, parent.transform);
    }

    public void OnPushedIntoStack()  { /* play enter animation, subscribe buttons */ }
    public void OnPoppedFromStack()  { /* play exit animation, then destroy */ }
    public void SetActive(bool active) => Instance.SetActive(active);
    public void Animate(string name, Action callback = null) { /* drive your Animator */ }
}
```

---

### Pushing an Element

```csharp
// Async push — callback fires once the asset is loaded and the element is live.
director.PushUiElementIntoCanvasCallback(
    element:      myPopup,
    type:         CanvasType.Front,
    mode:         StackVisibilityMode.AllVisible,
    blockingMode: InputBlockingMode.BlockBelowTop,
    callback:     () => myPopup.Animate("Enter")
);
```

```csharp
// Synchronous push (asset must already be loadable synchronously).
director.PushUiElementIntoCanvas(myElement, CanvasType.Middle);
```

---

### Popping an Element

```csharp
// Pop the top element unconditionally.
director.PopUiElementFromCanvas(CanvasType.Front);

// Pop only if the top element is of a specific type (safe, no-op otherwise).
director.PopUiElementFromCanvasIfMatch<MyPopupElement>(CanvasType.Front);

// Pop with a callback.
director.PopUiElementFromCanvasCallback(CanvasType.Front, () =>
{
    Debug.Log("Popup closed.");
});
```

---

### Inserting and Removing

```csharp
// Insert at a specific stack index (0 = bottom).
director.InsertUiElementInCanvas(element, index: 0, CanvasType.Back);

// Remove a specific element instance from anywhere in the stack.
director.RemoveUiElementFromCanvas(element, CanvasType.Back);
```

---

### Querying the Stack

```csharp
// Check if a canvas has any elements.
bool hasElements = director.AnyElementInCanvas(CanvasType.Front);

// Find the first element of a given type.
if (director.TryFindElementInCanvas<MyPopupElement>(out var popup, CanvasType.Front))
{
    popup.DoSomething();
}

// Find all elements of a given type.
if (director.TryFindElementsInCanvas<MyPopupElement>(out var popups, CanvasType.Front))
{
    foreach (var p in popups) p.DoSomething();
}
```

---

### Clearing and Destroying

```csharp
// Clear all elements from one canvas (calls OnPoppedFromStack on each).
director.ClearCanvas(CanvasType.System);

// Clear every canvas at once.
director.ClearAllCanvases();

// Fully destroy a canvas GameObject and remove it from the director.
director.DestroyCanvas(CanvasType.Loading);
```

---

### Screen Shake

```csharp
// Shake a single canvas.
director.ShakeCanvas(CanvasType.Middle, duration: 0.5f, magnitude: 8f, frequency: 25f);

// Shake every canvas simultaneously.
director.ShakeAllCanvases(duration: 1f, magnitude: 5f, frequency: 30f);
```

---

### Listening to Stack Events

`OnStackChanged` is exposed on `StackableUiCanvas`. Subscribe to it after `GenerateCanvases` if you need to react to any stack mutation:

```csharp
// Access the canvas through your director wrapper, then subscribe.
canvas.OnStackChanged += args =>
{
    Debug.Log($"{args.ChangeType} on canvas — " +
              $"stack size {args.OldStackSize} → {args.NewStackSize}");
};
```

`StackChangeType` values: `Pushed`, `Popped`, `Cleared`.

---

## ⚙️ Layer Hierarchy (`CanvasType`)

Layers are sorted automatically. Each type maps to a sorting order calculated as `BaseOrder + (OrderStep × (int)CanvasType)`.

| Layer | Intended use |
| :--- | :--- |
| `Background` | Environment backgrounds, panoramas |
| `Back` | Secondary panels behind the main UI |
| `Middle` | Primary gameplay UI |
| `Front` | Popups and modal dialogs |
| `Over` | Tooltips and floating notifications |
| `System` | Pause menus and global overlays, Error and System popups |
| `Loading` | Transition and loading screens — always on top |

---

## 🔧 Canvas Settings

`CanvasSetting` is a serializable struct you can configure in the Inspector or in code.

| Field | Description |
| :--- | :--- |
| `CanvasRenderMode` | `ScreenSpaceOverlay`, `ScreenSpaceCamera`, or `WorldSpace` |
| `ScaleMode` | `CanvasScaler.ScaleMode` (e.g. `ScaleWithScreenSize`) |
| `MatchMode` | `MatchWidthOrHeight`, `Expand`, or `Shrink` |
| `ReferenceResolution` | The resolution the UI was designed for |
| `Match` | Blend between width (0) and height (1) matching |
| `ReferencePixelsPerUnit` | Pixels-per-unit reference for the scaler |
| `BaseOrder` | Sorting order of the root canvas |
| `OrderStep` | Sorting order increment between each layer (must be > 0) |

**Built-in presets:**

```csharp
CanvasSetting.Default  // 1920×1080, match 0.5
CanvasSetting.Mobile   // 1080×1920, match width (0)
CanvasSetting.HighDpi  // 2560×1440, match 0.5, 200 ppu
```

Change settings at runtime without rebuilding canvases:

```csharp
director.ChangeCanvasResolution(CanvasSetting.Mobile);
```

---

## 📝 License

This project is released under the MIT License. Feel free to use and modify it for your own projects.
