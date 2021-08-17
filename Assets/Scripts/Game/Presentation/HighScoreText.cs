﻿using System.Globalization;
using Graphene.Game.Systems;
using Graphene.UiGenerics;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class HighScoreText : TextView
    {
        [Inject] private SignalBus _signalBus;

        private void Setup()
        {
            _signalBus.Subscribe<ScoreUpdate>(ScoreUpdate);
        }

        private void ScoreUpdate(ScoreUpdate session)
        {
            SetText(session.scoreSession.maxScore.ToString("000,000", CultureInfo.InvariantCulture));
        }
    }
}