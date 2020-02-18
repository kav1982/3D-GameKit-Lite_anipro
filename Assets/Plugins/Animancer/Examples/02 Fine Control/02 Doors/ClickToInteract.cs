// Animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer.Examples.FineControl
{
    /// <summary>可以与之交互的对象</summary>
    public interface IInteractable //Interactable可交互的
    {
        /************************************************************************************************************************/

        void Interact();

        /************************************************************************************************************************/
    }

    /// <summary>
    /// 尝试与用户单击鼠标时光标指向的任何<see cref="IInteractable"/> 进行交互
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Fine Control - Click To Interact")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.FineControl/ClickToInteract")]
    public sealed class ClickToInteract : MonoBehaviour
    {
        /************************************************************************************************************************/

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                var interactable = raycastHit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }

        /************************************************************************************************************************/
    }
}
