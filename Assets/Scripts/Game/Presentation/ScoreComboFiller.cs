using System;
using Graphene.Game.Systems;
using Graphene.UiGenerics;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class ScoreComboFiller : ImageView
    {
        [Inject] private ScoreSystem _scoreSystem;


        private void Update()
        {
            Image.fillAmount = _scoreSystem.CurrentComboStatusNormalized;
        }
    }
}