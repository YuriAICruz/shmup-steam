using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class WaveManager : MonoBehaviour
    {
        public WaveSpawner[] WaveSpawners;

        [Inject] private SignalBus _signalBus;
        private bool _ended;

        private void Awake()
        {
            _signalBus.Subscribe<LevelStarted>(Reset);
            _signalBus.Subscribe<GameOver>(Reload);
        }

        private void Reload(GameOver data)
        {
            //Todo: just temporary
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Reset(LevelStarted data)
        {
            _ended = false;
        }

        private void Update()
        {
            if (WavesEnded() && !_ended)
            {
                _ended = true;
                //_signalBus.Fire(new PlayerDeath());
                LevelUp();
            }
        }

        private void LevelUp()
        {
            for (int i = 0; i < WaveSpawners.Length; i++)
            {
                WaveSpawners[i].ResetAndUpgradeWave(1);
            }
            
            _ended = false;
        }

        private bool WavesEnded()
        {
            return WaveSpawners.All(x => x.Ended());
        }
    }
}