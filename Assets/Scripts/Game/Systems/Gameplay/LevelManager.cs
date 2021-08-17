using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class LevelStarted
    {
        public bool hasSector;
        public int sectorId;
        public Vector3 sectorPosition;

        public LevelStarted(bool hasSector, int sectorId, Vector3 sectorPosition)
        {
            this.hasSector = hasSector;
            this.sectorId = sectorId;
            this.sectorPosition = sectorPosition;
        }

        public LevelStarted(bool hasSector)
        {
            this.hasSector = hasSector;
        }
    }

    public class LevelEnded
    {
    }

    public class LevelCompleted
    {
    }
    
    public class GameOver
    {
    }

    public class SectorChange
    {
        public SectorSettings currentSectorSettings;

        public SectorChange(SectorSettings currentSectorSettings)
        {
            this.currentSectorSettings = currentSectorSettings;
        }
    }

    public class LevelManager
    {
        private readonly LevelSettings _levelSettings;
        private readonly SignalBus _signalBus;
        private readonly Delayer _delayer;
        private SectorSettings _currentSectorSettings;
        private SectorChange _sectorSignal;
        private LevelStarted _start;
        private SectorSettings _initialSectorSettings;

        private int _lives;
        public int Lives => _lives;

        public SectorSettings SectorSettings => _currentSectorSettings;

        public LevelManager(LevelSettings levelSettings, SignalBus signalBus, Delayer delayer,
            SectorSettings sectorSettings)
        {
            _levelSettings = levelSettings;
            _signalBus = signalBus;
            _delayer = delayer;

            _lives = (int) levelSettings.maxLives;

            _start = new LevelStarted(false);

            _currentSectorSettings = new SectorSettings();
            _initialSectorSettings = new SectorSettings();

            PoolTools.CloneValues(sectorSettings, _currentSectorSettings);
            PoolTools.CloneValues(sectorSettings, _initialSectorSettings);

            _sectorSignal = new SectorChange(_currentSectorSettings);

            _signalBus.Subscribe<PlayerDeath>(RestartLevel);
            _signalBus.Subscribe<BossDeath>(CompleteLevel);
        }

        private void CompleteLevel(BossDeath obj)
        {
            _signalBus.Fire<LevelCompleted>();
        }

        private void RestartLevel(PlayerDeath data)
        {
            if(!_levelSettings.infinityLives)
                _lives--;

            _signalBus.Fire<LevelEnded>();

            if (_currentSectorSettings != null)
            {
                _start.hasSector = true;
                _start.sectorId = _currentSectorSettings.uid;
                _start.sectorPosition = _currentSectorSettings.position;
            }
            else
            {
                _start.hasSector = false;
            }

            if (_lives <= 0)
            {
                _start.hasSector = false;
                _delayer.Delay(GameOver, _levelSettings.restartDelay);
            }

            _delayer.Delay(StartLevel, _levelSettings.restartDelay);
        }

        public void GameOver()
        {
            _signalBus.Fire<GameOver>();

            StartLevel();
        }

        public void StartLevel()
        {
            _signalBus.Fire(_start);

            if (!_start.hasSector)
            {
                _lives = (int) _levelSettings.maxLives;
                PoolTools.CloneValues(_initialSectorSettings, _currentSectorSettings);
            }

            _signalBus.Fire(_sectorSignal);
        }

        public void SetSector(SectorSettings sectorSettings)
        {
            PoolTools.CloneValues(sectorSettings, _currentSectorSettings);
            //_currentSectorSettings = sectorSettings;
            _signalBus.Fire(_sectorSignal);
        }
    }
}