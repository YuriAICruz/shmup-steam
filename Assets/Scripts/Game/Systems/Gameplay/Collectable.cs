using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public abstract class Collectable : MonoBehaviour, ICollectable, IPoolable
    {
        private bool _idle;
        
        public float speed = 2;
        
        protected Vector3 _direction;
        [Inject] protected CameraManager _cameraManager;
        [Inject] protected SignalBus _signalBus;


        [SerializeField]
        private uint _variation;

        public uint Variation => _variation;
        
        
        public bool Idle
        {
            get { return _idle; }
            protected set
            {
                if (_idle == value) return;

                _idle = value;
                if (_idle)
                {
                    transform.position = Vector3.one * -999;
                }
            }
        }

        protected virtual void Awake()
        {
            _signalBus.Subscribe<GameOver>(Reset);
        }

        private void Reset(GameOver data)
        {
            _idle = true;
        }

        private void Update()
        {
            if(Idle) return;

            Move();
            
            if (!_cameraManager.InBorders(transform.position))
            {
                Idle = true;
            }
        }

        public abstract void Move();

        public abstract void Collect();

        public virtual void Spawn(Vector3 position, Vector3 direction)
        {
            transform.position = position;
            _direction = direction;
            Idle = false;
        }
    }
}