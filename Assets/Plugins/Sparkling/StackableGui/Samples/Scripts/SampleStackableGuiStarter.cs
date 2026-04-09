using System.Collections;
using UnityEngine;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableGuiStarter : MonoBehaviour
    {
        private SampleStackableGuiDirector m_director;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);

            m_director = SampleStackableGuiDirector.Instance;

            SampleStackableElement initialElement = CreateStackableMenu();
            yield return new WaitUntil(() => initialElement.IsLoaded);
            initialElement.Animate("Enter");
        }

        private SampleStackableMenu CreateStackableMenu()
        {
            SampleStackableMenu menuElement = new SampleStackableMenu();
            m_director.PushUiElementIntoCanvas(menuElement, CanvasType.System);

            menuElement.SubscribeAddBackImage(AddBackImage);
            menuElement.SubscribeRemoveBackImage(RemoveBackImage);
            menuElement.SubscribeAddMiddleImage(AddMiddleImage);
            menuElement.SubscribeRemoveMiddleImage(RemoveMiddleImage);
            menuElement.SubscribeAddOverImage(AddOverImage);
            menuElement.SubscribeRemoveOverImage(RemoveOverImage);
            menuElement.SubscribeShakeScreen(ShakeScreen);
            return menuElement;
        }

        public void AddBackImage() => AddImage(new SampleStackableImage(), CanvasType.Back);
        public void RemoveBackImage() => RemoveImage(CanvasType.Back);

        public void AddMiddleImage() => AddImage(new SampleStackableImage(), CanvasType.Middle);
        public void RemoveMiddleImage() => RemoveImage(CanvasType.Middle);

        public void AddOverImage() => AddImage(new SampleStackableImage(), CanvasType.Over);
        public void RemoveOverImage() => RemoveImage(CanvasType.Over);


        private void AddImage(SampleStackableImage image, CanvasType canvas)
        {
            m_director.PushUiElementIntoCanvasCallback(image, canvas, StackVisibilityMode.AllVisible, InputBlockingMode.BlockNone, () =>
            {
                image.Animate("Enter");
                image.ChangeColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            });
        }

        private void RemoveImage(CanvasType canvas)
        {
            m_director.PopUiElementFromCanvas(canvas);
        }

        private void ShakeScreen()
        {
            m_director.ShakeAllCanvases(1f, 5f, 30f);
        }
    }
}