using Graphene.Game.Systems;
using Graphene.Game.Systems.Gameplay;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class PlayerLives : MonoBehaviour
    {
        [Inject] private ImageFactory _factory;
        [Inject] private Player _player;
        [Inject] private LevelManager _levelManager;
        [Inject] private LevelSettings _levelSettings;
        [Inject] private CameraManager _cameraManager;
        [Inject] private SignalBus _signalBus;
        private GameObject[] _lives;

        private void Awake()
        {
            if (_levelSettings.infinityLives)
            {
                gameObject.SetActive(false);
                return;
            }

            _lives = new GameObject[_levelSettings.maxLives];

            _signalBus.Subscribe<LevelEnded>(UpdateLives);
            _signalBus.Subscribe<GameOver>(ResetLives);

            for (int i = 0; i < _levelManager.Lives; i++)
            {
                _lives[i] = _factory.Create().gameObject;
                _lives[i].transform.SetParent(transform);
            }
        }

        private void ResetLives()
        {
            for (int i = 0; i < _lives.Length; i++)
            {
                _lives[i].SetActive(true);
            }
        }

        private void UpdateLives()
        {
            for (int i = 0; i < _lives.Length; i++)
            {
                _lives[i].SetActive(i < _levelManager.Lives);
            }
        }
    }
}