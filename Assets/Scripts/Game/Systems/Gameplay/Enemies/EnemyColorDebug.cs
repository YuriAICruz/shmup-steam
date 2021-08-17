using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class EnemyColorDebug : MonoBehaviour
    {
        public Renderer reference;

        private Enemy _enemy;
        private Material _material;

        [Inject] private Player _player;

        public Color normal = Color.white;
        public Color dashable = Color.yellow;
        public Color hasPowerup = Color.red;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();

            _material = reference.material;
        }

        private void Update()
        {
            SetColorFeedback();
        }

        private void SetColorFeedback()
        {
            if(_enemy.settings.powerUpProbability >= 1)
            {
                _material.color = hasPowerup;
                return;
            }
            
            if (_enemy.Hp <= _player.playerSettings.dashDamage)
            {
                _material.color = dashable;
                return;
            }
            
            _material.color = normal;
        }
    }
}