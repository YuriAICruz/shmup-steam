using System;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public abstract class MovingEnemy : Enemy, IPoolable
    {
        public enum EnemyType
        {
            Generic = 0,
            Linear = 1,
            LinearShooter = 2,
            Path = 5,
            SnapToCameraPoints = 10,
            MiniBossSphere = 50,
            BossSphere = 100
        }

        protected Vector3 _direction;
        protected float _speed;
        
        private bool _saw;
        private bool _idle;
        
        protected EnemyType _type;

        private Vector3 _position;

        protected MovingEnemyLogic _logic;
        private Vector3 _lookDir;

        [SerializeField]
        private uint _variation;

        public uint Variation => _variation;

        public bool Idle
        {
            get { return _idle; }
            private set
            {
                if (_idle == value) return;

                _idle = value;
                if (_idle)
                {
                    transform.position = Vector3.one * -999;
                }
            }
        }

        protected override void Awake()
        {
            gun = _gunFactory.Create(transform, gunSettings, Bullet.Type.Enemy);

            Idle = true;

            CreateLogic();

            _signalBus.Subscribe<LevelStarted>(SilentDie);
        }

        protected abstract void CreateLogic();

        public virtual bool CanSpawn(Spawner.SpawnData data)
        {
            return true;
        }

        public virtual void Spawn(Spawner.SpawnData data, EnemySettings overrideSettings = null, Gun.GunSettings gunOverrideSettings = null)
        {
            _saw = false;
            _position = _initialPosition = data.origin;
            Idle = false;

            if (overrideSettings != null)
                PoolTools.CloneValues(overrideSettings, settings);
            else
                PoolTools.CloneValues(InitialSettings, settings);

            if (gunOverrideSettings != null)
                gunSettings.Apply(gunOverrideSettings);
            else
                gunSettings.Apply(InitialGunSettings);

            _speed = settings.baseSpeed;

            ResetActor();
        }

        protected void Update()
        {
            if (Idle || _logic == null) return;

            RunBehaviour();
        }

        private void RunBehaviour()
        {
            _logic.Calculate(transform.position, out _direction, out _lookDir);

            Shoot();

            Move();
        }

        protected virtual void Shoot()
        {
            gun.Shoot(0);
        }

        private void Move()
        {
            if (settings.snapToCamera)
                _position += _cameraManager.LastMovement();
            
            _logic.Move(Time.deltaTime, _position, _speed, settings, out var pos, out var nd);
            
            _position = nd;
            
            transform.position = pos;
            transform.forward = _lookDir;
            var rot = Mathf.Rad2Deg * Mathf.Atan2(-_lookDir.y, -_lookDir.x);
            transform.rotation = Quaternion.Euler(new Vector3(rot, -90, 90));

            if (_cameraManager.InBorders(_position, CameraManager.BorderType.OffsetOut))
            {
                _saw = true;
            }
            else if (_saw)
            {
                Idle = true;
            }
        }

        protected override void Die()
        {
            base.Die();
            _logic.Die();
            Idle = true;
        }

        protected void SilentDie()
        {
            Idle = true;
        }

        public void SetType(EnemyType type)
        {
            _type = type;
        }
    }
}