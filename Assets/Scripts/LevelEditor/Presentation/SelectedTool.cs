using Graphene.UiGenerics;
using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor.Presentation
{
    public class SelectedTool : ImageView
    {
        private SignalBus _signalBus;

        private void Setup()
        {
            BrushReleased();
        }

        [Inject]
        private void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _signalBus.Subscribe<BrushSelected>(BrushSelected);
            _signalBus.Subscribe<BrushReleased>(BrushReleased);
        }

        private void BrushReleased()
        {
            Image.color = new Color(1,1,1,0.2f);
            Image.sprite = null;
        }

        private void BrushSelected(BrushSelected data)
        {
            Image.sprite = data.Icon;
            Image.color = Color.white;
        }
    }
}