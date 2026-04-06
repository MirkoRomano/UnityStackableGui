
# StackableGui

**StackableGui** is a robust, stack-based UI management system for Unity. It is designed with architectural decoupling and modularity in mind, allowing developers to manage complex UI hierarchies across multiple layers with ease.

The system treats different UI sections (Background, Gameplay, Popups, etc.) as independent stacks, automating rendering order, input blocking, and Safe Area calculations.

## 🚀 Key Features

* **Stack-Based Navigation**: Naturally manage UI flow using Push, Pop, and Insert operations.
* **Automatic Safe Area Management**: Native support for mobile notches and screen cutouts, dynamically calculated based on the `RenderMode`.
* **Abstract Asset Loading**: Use the `IUiAssetLoader` interface to easily integrate Addressables, Resources, or custom pooling solutions.
* **Multi-Canvas Layering**: Organize UI via `CanvasType` (Background, Middle, Front, System, etc.) with automatic sorting order management.
* **Visibility & Input Control**: Built-in modes to hide underlying stack elements or block input on lower layers.
* **Event-Driven Workflow**: Fully decoupled communication through the `OnStackChanged` event and `StackChangedEventArgs`.

---

## 🛠 Project Structure

| File | Description |
| :--- | :--- |
| `IStackableUiDirector` | The main API to command the entire UI system. |
| `StackableUiCanvas` | Handles local stack logic, sorting, and visibility for a specific layer. |
| `CanvasSetting` | A serializable struct to define resolution, match modes, and render settings. |
| `IStackableUIElement` | The interface for your UI controllers (e.g., a `MainMenuController`). |

---

## 📦 Installation

1. Copy the source files into your Unity project's `Assets/` folder.
2. Implement the `IUiAssetLoader` interface to bridge your preferred loading system:

```csharp
public class MyResourcesLoader : IUiAssetLoader {
    public void LoadAssetAsync(string path, Action<GameObject> onLoaded, Action<Exception> onError = null) {
        var prefab = Resources.Load<GameObject>(path);
        onLoaded?.Invoke(prefab);
    }
    // ... implement remaining methods
}
```

---

## 📋 Quick Start

### Initialization
```csharp
// Initialize the director with a loader and a specific setting preset
var director = new StackableUiDirector(new MyResourcesLoader(), CanvasSetting.Default);

// Generate the canvas hierarchy in the scene
director.GenerateCanvases(null); 
director.ApplySafeArea();
```

### Pushing an Element
```csharp
// Load and push a UI element into the "Front" layer (e.g., a Popup)
director.PushUiElementIntoCanvasCallback(
    myElement, 
    CanvasType.Front, 
    StackVisibilityMode.TopOnly, 
    InputBlockingMode.BlockBelowTop
);
```

---

## ⚙️ Layer Hierarchy (`CanvasType`)
The system automatically organizes UI depth according to the following hierarchy:
1.  **Background**: Environment backgrounds or panoramas.
2.  **Back**: Secondary panels behind the main UI.
3.  **Middle**: Primary gameplay UI.
4.  **Front**: Popups and modal dialogs.
5.  **Over**: Tooltips and floating notifications.
6.  **System**: Pause menus and global settings.
7.  **Loading**: Always on top; intended for transition screens.

---

## 📝 License
This project is released under the MIT License. Feel free to use and modify it for your own projects!

---
