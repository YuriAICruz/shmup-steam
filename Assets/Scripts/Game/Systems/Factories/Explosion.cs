using Graphene.Game.Systems.Gameplay;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Factories
{
    public class Explosion : MonoBehaviour
    {
        private float _duration;

        [Inject] private Delayer _delayer;

        public class Factory : PlaceholderFactory<Explosion>
        {
            public Explosion Create(Vector3 position, float duration = 1)
            {
                var ex = Create();
                ex.Setup(position, duration);
                return ex;
            }
        }

        private void Setup(Vector3 position, float duration)
        {
            transform.position = position;
            _duration = duration;

            _delayer.Delay(() => { Destroy(gameObject); }, duration);
        }
    }
}