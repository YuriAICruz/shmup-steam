using Graphene.Game.Systems.Input;
using Graphene.UiGenerics;
using UnityEngine.UI;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class SwitchMouseJoystickButton : ButtonView
    {
        [Inject] private InputSettings _inputSettings;
        private Text _tx;

        private void Setup()
        {
            _tx = transform.GetComponentInChildren<Text>();

            SetText();
        }

        private void SetText()
        {
            _tx.text = _inputSettings.Mouse ? "Mouse" : "Joystick";
        }

        protected override void OnClick()
        {
            base.OnClick();

            _inputSettings.SetUseMouse(!_inputSettings.Mouse);
            SetText();
        }
    }
}