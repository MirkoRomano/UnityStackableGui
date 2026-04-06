using System;
using UnityEngine;

namespace Sparkling.StackableGui
{
    public interface IStackableUIElement
    {
        string Path { get; }
        bool IsVisible { get; }
        bool IsLoaded { get; }
        bool IsAnimating { get; }
        GameObject Instance { get; }
        GameObject Prefab { get; }
        GameObject Parent { get; }
        void Initialize(GameObject prefab, GameObject parent);
        void OnPushedIntoStack();
        void OnPoppedFromStack();
        void SetActive(bool active);
        void Animate(string animationName, Action callback = null);
    }
}
