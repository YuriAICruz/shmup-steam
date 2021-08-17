using Graphene.UiGenerics;
using Zenject;

namespace Graphene.LevelEditor.Presentation.Button
{
    public class SubMenuButton : ButtonView
    {
        public MenuWindow menu;

        [Inject] private SignalBus _signalBus;
        [Inject] private EditorSettings _settings;

        void Setup()
        {
            _signalBus.Subscribe<MenuWindowOpen>(UpdateButton);

            Button.image.color = _settings.normalColor;
        }

        private void UpdateButton(MenuWindowOpen data)
        {
            Button.image.color = data.Menu == menu ? _settings.selectedColor : _settings.normalColor;
        }

        protected override void OnClick()
        {
            base.OnClick();

            _signalBus.Fire(new MenuWindowOpen(menu));
        }
    }
}