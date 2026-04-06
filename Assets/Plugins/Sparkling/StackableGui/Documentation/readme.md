# StackableGui

Stack-based UI management system. Organizes UI elements across layered canvases with built-in visibility, input blocking, and asset loading.

---

## Structure

```
Runtime/
├── Core/           # StackableUiCanvas, StackableUiDirector
├── Interfaces/     # IStackableUiDirector, IStackableUIElement, IUiAssetLoader
└── Data/           # CanvasSetting, GlobalEnums, StackChangedEventArgs
Samples/
└── BasicSample/    # SampleStackableGuiDirector — Resources-based reference implementation
```

---

## Setup

1. Implement `IUiAssetLoader` for your loading system (Resources, Addressables, etc.)
2. Implement `IStackableUIElement` for each UI screen or panel
3. Add a MonoBehaviour that implements `IStackableUiDirector` to your scene (see `SampleStackableGuiDirector`)
4. Configure `CanvasSetting` in the Inspector — presets available: `Default`, `Mobile`, `HighDpi`

---

## Canvas Layers

Ordered bottom to top: `Background → Back → Middle → Front → Over → System → Loading`

---

## Element Lifecycle

```
Initialize(prefab) → OnPushedIntoStack() → [SetActive / OnEnable / OnDisable] → OnPoppedFromStack() → ReleasePrefab()
```

- Destroy the instance inside `OnPoppedFromStack`, optionally after an animation
- `ReleasePrefab` is called by the system immediately after `OnPoppedFromStack`

---

## Notes

- `StackVisibilityMode.TopOnly` hides all elements below the top of the stack
- `InputBlockingMode.BlockBelowTop` disables raycasting on all canvases except the topmost active one
- Call `ApplySafeArea()` on orientation change or every frame if needed
- Prefer async push/pop methods in production to avoid main-thread hitches