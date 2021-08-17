using System;
using System.Reflection;
using Graphene.Game.Systems.Gameplay;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems
{
    public class ScoreUpdate
    {
        public ScoreSystem.ScoreSession scoreSession;

        public ScoreUpdate(ScoreSystem.ScoreSession currentSession, ScoreSystem.ScoreSettings settings)
        {
            scoreSession = new ScoreSystem.ScoreSession(settings.comboDuration);
            SetValues(currentSession);
        }

        public void SetValues(ScoreSystem.ScoreSession currentSession)
        {
            var parms = currentSession.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < parms.Length; i++)
            {
                parms[i].SetValue(scoreSession, parms[i].GetValue(currentSession));
            }

            scoreSession.score = currentSession.score;
        }
    }

    public class ScoreSystem
    {
        [Serializable]
        public class ScoreSettings
        {
            public uint baseHitScore = 1;
            public uint baseKillScore = 1;
            public uint baseDashKillScore = 1;
            public uint baseComboScoreStep = 1;
            public ulong baseDashReflectScore = 1;
            public uint maxCombo = 8;
            public float comboDuration = 1.2f;
        }

        [Serializable]
        public class ScoreSession
        {
            private readonly float _comboDuration;
            public float ComboDuration => _comboDuration;
            public string name;
            private ulong _score;
            private float _lastTime;
            private bool _reseted;

            public Action OnComboReset;

            public ScoreSession(float comboDuration)
            {
                _comboDuration = comboDuration;
                _lastTime = -_comboDuration;
            }

            public float ComboStatusNormalized
            {
                get
                {
                    var status = 1 - Mathf.Clamp01((Time.time - _lastTime) / _comboDuration);

                    if (status <= 0)
                    {
                        if (!_reseted)
                        {
                            _reseted = true;
                            OnComboReset?.Invoke();   
                        }
                    }
                    else
                    {
                        _reseted = false;
                    }

                    return status;
                }
            }

            public ulong score
            {
                get { return _score; }
                set
                {
                    _lastTime = Time.time;
                    _score = value;
                }
            }

            public ulong rawScore;
            public ulong maxScore;
            public ulong kills;
            public ulong dashKills;
            public ulong maxCombo;
            public ulong deaths;
            public ulong hits;
            public ulong playerHits;
            public ulong shoots;
            public ulong dashReflect;
        }

        private ScoreSession _currentSession;
        private ulong _currentComboScore;

        private ScoreUpdate _signal;

        private ulong Combo => (ulong) Mathf.Clamp(
            (_currentSession.rawScore - _currentComboScore) / (_settings.baseComboScoreStep),
            0,
            _settings.maxCombo
        );


        ulong _baseCombo;

        private readonly SignalBus _signalBus;
        private readonly ScoreSettings _settings;

        public float CurrentComboStatusNormalized => _currentSession.ComboStatusNormalized;

        public ScoreSystem(SignalBus signalBus, ScoreSettings settings)
        {
            _signalBus = signalBus;
            _settings = settings;

            CreateSession();

            _signal = new ScoreUpdate(_currentSession, _settings);

            _signalBus.Subscribe<BulletShoot>(AddShootStatistic);
            _signalBus.Subscribe<BulletHit>(AddHitScore);
            _signalBus.Subscribe<BulletKill>(AddKillScore);
            _signalBus.Subscribe<DashKill>(AddDashScore);
            _signalBus.Subscribe<DashReflect>(AddDashReflect);
            _signalBus.Subscribe<PlayerHit>(OnPlayerHit);
            _signalBus.Subscribe<PlayerDeath>(AddDeathStatistic);
            _signalBus.Subscribe<GameOver>(OnGameOver);
        }

        private void CreateSession()
        {
            // if (_currentSession != null)
            // {
            //     _currentSession.OnComboReset -= ResetCombo;
            // }
            _currentSession = new ScoreSession(_settings.comboDuration);
            _currentSession.OnComboReset += ResetCombo;
        }

        public void StartSession()
        {
            if (_currentSession != null)
                throw new NotImplementedException();

            CreateSession();
            ResetCombo();
            SendSignal();
        }

        private void SendSignal()
        {
            _signal.SetValues(_currentSession);
            _signalBus.Fire(_signal);
        }

        public void SetSessionName(string name)
        {
            _currentSession.name = name;
            SendSignal();
        }

        private void ResetScore()
        {
            if (_currentSession.score > _currentSession.maxScore)
                _currentSession.maxScore = _currentSession.score;

            if (Combo > _currentSession.maxCombo)
                _currentSession.maxCombo = Combo;

            _currentSession.score = 0;
            _currentSession.rawScore = 0;

            ResetCombo();
        }

        private void ResetCombo()
        {
            if (Combo > _currentSession.maxCombo)
                _currentSession.maxCombo = Combo;
            
            _currentComboScore = _currentSession.rawScore;

            SendSignal();
        }

        #region Events

        private void AddDeathStatistic(PlayerDeath data)
        {
            _currentSession.deaths++;

            ResetCombo();
            //ResetScore();
        }

        private void OnGameOver(GameOver data)
        {
            ResetScore();
        }

        private void AddShootStatistic(BulletShoot data)
        {
            _currentSession.shoots++;
        }

        private void AddHitScore(BulletHit data)
        {
            _currentSession.hits++;

            _currentSession.score += _settings.baseHitScore + _settings.baseHitScore * Combo;
            _currentSession.rawScore += _settings.baseHitScore;
            SendSignal();
        }

        private void AddKillScore(BulletKill data)
        {
            _currentSession.kills++;

            _currentSession.score += _settings.baseKillScore + _settings.baseKillScore * Combo;
            _currentSession.rawScore += _settings.baseKillScore;
            SendSignal();
        }

        private void AddDashScore(DashKill data)
        {
            _currentSession.dashKills++;

            _currentSession.score += _settings.baseDashKillScore + _settings.baseDashKillScore * Combo;
            _currentSession.rawScore += _settings.baseDashKillScore;
            SendSignal();
        }

        private void AddDashReflect(DashReflect data)
        {
            _currentSession.dashReflect++;

            _currentSession.score += _settings.baseDashReflectScore + _settings.baseDashReflectScore * Combo;
            _currentSession.rawScore += _settings.baseDashReflectScore;
            SendSignal();
        }

        private void OnPlayerHit(PlayerHit data)
        {
            _currentSession.playerHits++;

            ResetCombo();
        }

        #endregion

        public ulong GetHighScore()
        {
            return _currentSession.maxScore;
        }

        public ulong GetCurrentCombo()
        {
            return Combo;
        }
    }
}