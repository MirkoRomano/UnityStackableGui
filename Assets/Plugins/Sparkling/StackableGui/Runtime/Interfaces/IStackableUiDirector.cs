using System;
using UnityEngine;

namespace Sparkling.StackableGui
{
    public interface IStackableUiDirector
    {
        bool Initialized { get; }
        bool HasSafeArea { get; }
        void GenerateCanvases(Transform parent);
        void ApplySafeArea();
        void PushUiElementIntoCanvas(IStackableUIElement element,
                                     CanvasType type,
                                     StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                     InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void PushUiElementIntoCanvasCallback(IStackableUIElement element,
                                             CanvasType type,
                                             StackVisibilityMode mode,
                                             InputBlockingMode blockingMode = InputBlockingMode.BlockNone,
                                             Action callback = null);
        void PopUiElementFromCanvas(CanvasType type,
                                    StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                    InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void PopUiElementFromCanvasIfMatch<T>(CanvasType type,
                                              StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                              InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void PopUiElementFromCanvasCallback(CanvasType type,
                                            Action callback,
                                            StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                            InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void PopUiElementFromCanvasCallbackIfMatch<T>(CanvasType type,
                                                      Action callback,
                                                      StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                                      InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void InsertUiElementInCanvas(IStackableUIElement element,
                                     int index,
                                     CanvasType type,
                                     StackVisibilityMode mode = StackVisibilityMode.AllVisible);
        void RemoveUiElementFromCanvas(IStackableUIElement element,
                                       CanvasType type,
                                       StackVisibilityMode mode = StackVisibilityMode.AllVisible,
                                       InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void ClearCanvas(CanvasType type, InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        void ClearAllCanvases(InputBlockingMode blockingMode = InputBlockingMode.BlockNone);
        bool AnyElementInCanvas(CanvasType type);
        bool TryFindElementInCanvas<T>(out T element, CanvasType type) where T : IStackableUIElement;
        bool TryFindElementsInCanvas<T>(out T[] elements, CanvasType type) where T : IStackableUIElement;
        void ChangeCanvasResolution(CanvasSetting settings);
        void ShakeCanvas(CanvasType type, float duration, float magnitude, float frequency);
        void ShakeAllCanvases(float duration, float magnitude, float frequency);
        void DestroyCanvas(CanvasType type);
    }
}