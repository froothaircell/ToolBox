using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ToolBox.Managers.InputManagement;

namespace ToolBox.UIViews.BaseScripts
{
    [RequireComponent(typeof(Selectable))]
    public class SliderSelectionButton : UIButton_PersistentPress, ISelectHandler, IDeselectHandler
    {
        [SerializeField]
        private Slider _slider;

        public void OnSelect(BaseEventData eventData)
        {
            if (InputManager.InputActions != null)
                InputManager.InputActions.UI.Submit.performed += OnClick;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (InputManager.InputActions != null)
                InputManager.InputActions.UI.Submit.performed -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed && InputManager.InputActions != null)
            {
                InputManager.InputActions.UI.Submit.performed += OnCancel;
                InputManager.InputActions.UI.Cancel.performed += OnCancel;
            }
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (InputManager.InputActions != null)
            {
                InputManager.InputActions.UI.Submit.performed -= OnCancel;
                InputManager.InputActions.UI.Cancel.performed -= OnCancel;
            }
        }

    }
}
