using System;
using UnityEngine;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackablePopup : IStackableUIElement
    {
        public string Path => throw new NotImplementedException();

        public bool IsVisible => throw new NotImplementedException();

        public bool IsLoaded => throw new NotImplementedException();

        public bool IsAnimating => throw new NotImplementedException();

        public GameObject Instance => throw new NotImplementedException();

        public GameObject Prefab => throw new NotImplementedException();

        public GameObject Parent => throw new NotImplementedException();

        public void Animate(string animationName, Action callback = null)
        {
            throw new NotImplementedException();
        }

        public void Initialize(GameObject prefab, GameObject parent)
        {
            throw new NotImplementedException();
        }

        public void OnPoppedFromStack()
        {
            throw new NotImplementedException();
        }

        public void OnPushedIntoStack()
        {
            throw new NotImplementedException();
        }

        public void SetActive(bool active)
        {
            throw new NotImplementedException();
        }
    }
}

