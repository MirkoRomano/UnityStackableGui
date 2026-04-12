using UnityEngine;
using UnityEngine.UI;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableImage : SampleStackableElement
    {
        public override string Path => "SamplePrefabs/SampleStackableImage_Pref";
        public override bool IsLoaded => m_renderer && !m_renderer.cull;
        private CanvasRenderer m_renderer;
        private Image m_image;

        public override void Initialize(GameObject prefab, GameObject parent)
        {
            base.Initialize(prefab, parent);
            m_renderer = m_instance.GetComponentInChildren<CanvasRenderer>();
            m_image = m_instance.GetComponentInChildren<Image>();
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


        public void ChangeColor(Color color)
        {
            m_image.color = color;
        }
    }
}

