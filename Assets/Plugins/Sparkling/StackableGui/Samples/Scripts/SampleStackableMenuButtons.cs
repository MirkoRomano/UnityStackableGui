using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableMenuButtons : MonoBehaviour
    {
        [Serializable]
        public class ImageButton
        {
            public Button Add;
            public Button Remove;
        }

        [SerializeField]
        private ImageButton m_backButton;

        [SerializeField]
        private ImageButton m_middleButton;

        [SerializeField]
        private ImageButton m_overButton;

        [SerializeField]
        private Button m_shakeButton;

        public void SubscribeShakeButton(UnityAction action)
        {
            m_shakeButton.onClick.AddListener(action);
        }

        public void SubscribeButton(CanvasType type, UnityAction action)
        {
            ImageButton button = GetButton(type);

            if(button == null)
            {
                return;
            }

            button.Add.onClick.AddListener(action);
        }

        public void RemoveButton(CanvasType type, UnityAction action)
        {
            ImageButton button = GetButton(type);

            if (button == null)
            {
                return;
            }

            button.Remove.onClick.AddListener(action);
        }

        private ImageButton GetButton(CanvasType type)
        {
            return type switch
            {
                CanvasType.Back => m_backButton,
                CanvasType.Middle => m_middleButton,
                CanvasType.Over => m_overButton,
                _ => null
            };
        }
    }
}