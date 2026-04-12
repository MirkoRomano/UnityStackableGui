using System.Collections;
using UnityEngine;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableGuiStarter : MonoBehaviour
    {
        private SampleStackableGuiDirector m_director => SampleStackableGuiDirector.Instance;

        IEnumerator Start()
        {
            yield return new WaitUntil(() => m_director != null);
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
            menuElement.SubscribeAddFrontImage(AddFrontImage);
            menuElement.SubscribeRemoveFrontImage(RemoveFrontImage);
            menuElement.SubscribeAddOverImage(AddOverImage);
            menuElement.SubscribeRemoveOverImage(RemoveOverImage);
            menuElement.SubscribeShakeScreen(ShakeScreen);
            menuElement.SubscribeShowPopup(ShowPopup);
            return menuElement;
        }

        public void AddBackImage() => AddImage(new SampleStackableImage(), CanvasType.Back);
        public void RemoveBackImage() => RemoveImage(CanvasType.Back);

        public void AddMiddleImage() => AddImage(new SampleStackableImage(), CanvasType.Middle);
        public void RemoveMiddleImage() => RemoveImage(CanvasType.Middle);

        public void AddFrontImage() => AddImage(new SampleStackableImage(), CanvasType.Front);
        public void RemoveFrontImage() => RemoveImage(CanvasType.Front);

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
            m_director.PopUiElementFromCanvasIfMatch<SampleStackableImage>(canvas);
        }

        public void ShowPopup()
        {
            SampleStackablePopup popup = new SampleStackablePopup();
            m_director.PushUiElementIntoCanvasCallback(popup, CanvasType.System, StackVisibilityMode.AllVisible, InputBlockingMode.BlockNone, () =>
            {
                popup.Animate("Enter");
                popup.SubscribeConfirmButton(() =>
                {
                    m_director.PopUiElementFromCanvasIfMatch<SampleStackablePopup>(CanvasType.System);
                });
            });
        }

        private void ShakeScreen()
        {
            m_director.ShakeAllCanvases(1f, 5f, 30f);
        }
    }
}