using Graphene.UiGenerics;
using Zenject;

namespace Graphene.LevelEditor.Presentation.Button
{
    public class PlayButton : ButtonView
    {
        [Inject] private LevelManager _levelManager;

        protected override void OnClick()
        {
            Button.interactable = false;
            _levelManager.Run(() => { Button.interactable = true; });
        }
    }
}