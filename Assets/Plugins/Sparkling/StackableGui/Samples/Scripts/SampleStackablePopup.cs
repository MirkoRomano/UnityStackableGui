using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackablePopup : SampleStackableElement
    {
        public override string Path => "SamplePrefabs/SampleStackablePopup_Pref";
        public override bool IsLoaded => m_renderer && !m_renderer.cull;
        
        private CanvasRenderer m_renderer;
        private Button m_confirmButton;

        public override void Initialize(GameObject prefab, GameObject parent)
        {
            base.Initialize(prefab, parent);
            m_renderer = m_instance.GetComponentInChildren<CanvasRenderer>();
            m_confirmButton = m_instance.transform.Find("Panel/Footer/Button (Legacy)").GetComponent<Button>();
            ForceResetanimation();
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

        public void SubscribeConfirmButton(UnityAction action)
        {
            m_confirmButton.onClick.AddListener(action);
        }
    }
}

