using System.Globalization;
using Graphene.Game.Systems;
using Graphene.UiGenerics;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class CurrentScoreText : TextView
    {
        [Inject] private SignalBus _signalBus;

        private void Setup()
        {
            _signalBus.Subscribe<ScoreUpdate>(ScoreUpdate);
        }

        private void ScoreUpdate(ScoreUpdate session)
        {
            SetText(session.scoreSession.score.ToString("000,000", CultureInfo.InvariantCulture));
        }
    }
}