using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class Bullet : MonoBehaviour, IPoolable
    {
        [Serializable]
        public class BulletSettings
        {
            public float speed = 6;
            public float baseScale = 0.3f;
            public float scaleMul = 1;
            public float maxLife = 3.5f;
            
            public bool snapToCamera;

            public LayerMask layer;
        }

        public BulletSettings bulletSettings;

        [SerializeField]
        private uint _variation;

        public uint Variation => _variation;

        public enum Type
        {
            Generic = 0,
            Player = 1,
            Enemy = 2
        }

        public class Factory : PlaceholderFactory<Bullet>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public Bullet Create(Type type)
            {
                if (type == Type.Generic)
                    return Create();

                var bullet = _container.InstantiatePrefabResource("Bullet/" + type.ToString()).GetComponent<Bullet>();
                bullet.Setup(type);
                return bullet;
            }
        }

        private void Setup(Type type)
        {
            _type = type;
        }

        private PhysicsTools _physics;

        [Inject] private CameraManager _cameraManager;
        [Inject] private SignalBus _signalBus;

        private Vector3 _direction;
        private float _life;

        private BulletSettings _initialBulletSettings;

        private bool _idle;
        private int _charge;
        private int baseDamage = 1;
        private Type _type;

        private Queue<Action> _listeners = new Queue<Action>();

        public bool Idle
        {
            get { return _idle; }
            private set
            {
                if (_idle == value) return;

                _idle = value;
                if (_idle)
                {
                    for (int i = 0, n = _listeners.Count; i < n; i++)
                    {
                        _listeners.Dequeue()();
                    }

                    _physics.Reset();
                    transform.position = Vector3.one * -99999;
                }
            }
        }

        private void Awake()
        {
            _physics = new PhysicsTools(transform);
            Idle = true;

            _initialBulletSettings = new BulletSettings();
            PoolTools.CloneValues(bulletSettings, _initialBulletSettings);

            _signalBus.Subscribe<LevelStarted>(DisableSelf);
        }

        private void DisableSelf()
        {
            Idle = true;
        }

        public void ResetSettings()
        {
            PoolTools.CloneValues(_initialBulletSettings, bulletSettings);
        }

        public void SetBulletSettings(BulletSettings newBulletSettings)
        {
            PoolTools.CloneValues(newBulletSettings, bulletSettings);
        }

        public void Shoot(Vector3 position, Vector3 dir, int charge, Action ended)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            Idle = false;
            transform.position = position;
            _direction = dir;
            _life = 0;
            _charge = charge;
            SetSize(charge);

            if (ended != null)
                _listeners.Enqueue(ended);
        }

        private void SetSize(int charge)
        {
            transform.localScale = Vector3.one *
                                   (bulletSettings.baseScale +
                                    bulletSettings.baseScale * charge * bulletSettings.scaleMul);
        }

        private void Update()
        {
            if (Idle) return;

            _life += Time.deltaTime;

            var pos = transform.position + _direction * (bulletSettings.speed * Time.deltaTime);

            if (bulletSettings.snapToCamera)
            {
                pos += _cameraManager.LastMovement();
            }
            
            if (!_cameraManager.InBorders(pos, CameraManager.BorderType.OffsetOut))
            {
                Idle = true;
                return;
            }

            transform.position = pos;

            transform.forward = _direction;

            if (_life >= bulletSettings.maxLife)
                Idle = true;
        }

        private void FixedUpdate()
        {
            if (Idle) return;

            var radius = transform.localScale.x;

            if (_physics.CheckCollisions(radius, bulletSettings.layer, out var hit))
            {
                if (hit.GetComponent<IDamageable>() != null && hit.GetComponent<IDamageable>().DoDamage(baseDamage + baseDamage * _charge))
                {
                    _charge -= 1;
                    SetSize(_charge);
                }
                else
                {
                    _charge = 0;
                }
                    
                if (_charge <= 0)
                    Idle = true;
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, transform.localScale.x);
#endif
        }
    }
}