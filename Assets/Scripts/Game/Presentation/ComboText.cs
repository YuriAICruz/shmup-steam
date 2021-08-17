using System.Globalization;
using Graphene.Game.Systems;
using Graphene.UiGenerics;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class ComboText : TextView
    {
        [Inject] private ScoreSystem _score;
        [Inject] private SignalBus _signalBus;

        private void Setup()
        {
            _signalBus.Subscribe<ScoreUpdate>(ScoreUpdate);
        }

        private void ScoreUpdate(ScoreUpdate session)
        {
            SetText("x"+ (_score.GetCurrentCombo()+1).ToString("0", CultureInfo.InvariantCulture));
        }
    }
}