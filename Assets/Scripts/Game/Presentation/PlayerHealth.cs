using Graphene.Game.Systems;
using Graphene.Game.Systems.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class ImageFactory : PlaceholderFactory<Image>
    {
    }
    public class PlayerHealth : MonoBehaviour
    {
        [Inject] private ImageFactory _factory;
        [Inject] private Player _player;
        [Inject] private CameraManager _cameraManager;
        [Inject] private SignalBus _signalBus;
        private GameObject[] _hps;

        private void Awake()
        {
            _hps = new GameObject[_player.maxHp];

            _player.OnHit += UpdateHp;
            _signalBus.Subscribe<LevelStarted>(ResetHp);

            for (int i = 0; i < _player.maxHp; i++)
            {
                _hps[i] = _factory.Create().gameObject;
                _hps[i].transform.SetParent(transform);
            }
        }

        private void ResetHp()
        {
            for (int i = 0; i < _hps.Length; i++)
            {
                _hps[i].SetActive(true);
            }
        }

        private void UpdateHp()
        {
            for (int i = 0; i < _hps.Length; i++)
            {
                _hps[i].SetActive(i < _player.Hp);
            }
        }
    }
}