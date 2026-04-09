using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableMenu : SampleStackableElement
    {
        public override string Path => "SamplePrefabs/SampleStackableMenu_Pref";
        public override bool IsLoaded => m_renderer && !m_renderer.cull;
        private CanvasRenderer m_renderer;
        private SampleStackableMenuButtons m_buttons;

        public override void Initialize(GameObject prefab, GameObject parent)
        {
            base.Initialize(prefab, parent);
            m_renderer = m_instance.GetComponentInChildren<CanvasRenderer>();
            m_buttons = m_instance.GetComponent<SampleStackableMenuButtons>();

            m_animator.Update(0f);
        }

        public override void OnPushedIntoStack()
        {
            if (!SampleStackableGuiDirector.Active)
            {
                return;
            }

            Animate("Out");
        }

        public override void OnPoppedFromStack()
        {
            if (!SampleStackableGuiDirector.Active)
            {
                return;
            }

            Animate("Exit", Destroy);
        }

        public void SubscribeAddBackImage(UnityAction action)
        {
            m_buttons.SubscribeButton(CanvasType.Back, action);
        }

        public void SubscribeRemoveBackImage(UnityAction action) 
        {
            m_buttons.RemoveButton(CanvasType.Back, action);
        }

        public void SubscribeAddMiddleImage(UnityAction action)
        {
            m_buttons.SubscribeButton(CanvasType.Middle, action);
        }

        public void SubscribeRemoveMiddleImage(UnityAction action)
        {
            m_buttons.RemoveButton(CanvasType.Middle, action);
        }

        public void SubscribeAddOverImage(UnityAction action)
        {
            m_buttons.SubscribeButton(CanvasType.Over, action);
        }

        public void SubscribeRemoveOverImage(UnityAction action)
        {
            m_buttons.RemoveButton(CanvasType.Over, action);
        }

        public void SubscribeShakeScreen(UnityAction action)
        {
            m_buttons.SubscribeShakeButton(action);
        }
    }
}