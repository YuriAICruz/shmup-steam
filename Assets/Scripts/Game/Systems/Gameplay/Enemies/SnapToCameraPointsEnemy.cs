using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class SnapToCameraPointsEnemy : MovingEnemy
    {
        [Header("SnapToCameraPointsEnemy Options")]
        public float burstDuration;

        public float burstCooldown;

        [Inject] private LevelManager _levelManager;
        private float _time;
        private float _timeCooldown;
        private bool _stoped;

        public class SnapToCameraPointsEnemyLogic : MovingEnemyLogic
        {
            private CameraFollowPoints _destination;
            private Vector3 _exitDirection;
            private bool _exiting;
            private bool _stoped;
            public bool Stoped => _stoped;

            public SnapToCameraPointsEnemyLogic() : base(Vector3.zero)
            {
                _destination = null;
                _exitDirection = Vector3.zero;
            }

            public SnapToCameraPointsEnemyLogic(Vector3 direction, CameraFollowPoints destination,
                Vector3 exitDirection) : base(direction)
            {
                _destination = destination;
                _exitDirection = exitDirection;
            }

            public void Reset(Vector3 direction, CameraFollowPoints destination, Vector3 exitDirection)
            {
                _exitDirection = exitDirection;
                _destination = destination;
                _stoped = false;

                Reset(direction);
            }

            public override void Reset(Vector3 direction)
            {
                base.Reset(direction);
                _exiting = false;
                _stoped = false;
            }

            public override void Die()
            {
                if (_destination)
                    _destination.Release();
            }

            public override void Calculate(Vector3 position, out Vector3 direction, out Vector3 lookDir)
            {
                lookDir = Vector3.up;
                direction = _direction;

                if (_destination == null || _destination.Idle)
                {
                    if (!_exiting)
                    {
                        _exiting = true;
                        direction = _direction = _exitDirection;
                    }

                    return;
                }

                _direction = (_destination.transform.position - position);

                if (_direction.sqrMagnitude > 0.05f)
                {
                    _direction.Normalize();
                    //lookDir = _direction;
                }
                else
                {
                    _stoped = true;
                    _settings.snapToCamera = true;
                }

                direction = _direction;
            }
        }

        public override void Spawn(Spawner.SpawnData data, EnemySettings overrideSettings = null,
            Gun.GunSettings gunOverrideSettings = null)
        {
            base.Spawn(data, overrideSettings, gunOverrideSettings);

            if (_levelManager.SectorSettings.GetNextCloserCameraPoint(data.origin, data.maxDistance, out var temp))
            {
                temp.Pick();
                _direction = (temp.transform.position - transform.position).normalized;
                ((SnapToCameraPointsEnemyLogic) _logic).Reset(_direction, temp, settings.exitDirection);
            }
            else
            {
                SilentDie();
            }
        }

        public override bool CanSpawn(Spawner.SpawnData data)
        {
            return _levelManager.SectorSettings.GetNextCloserCameraPoint(data.origin, data.maxDistance, out var temp);
        }

        protected override void CreateLogic()
        {
            _logic = new SnapToCameraPointsEnemyLogic(_direction, null, Vector3.zero);
        }

        protected override void Shoot()
        {
            if (Time.time - _timeCooldown < burstCooldown)
                return;

            if (Time.time - _time < burstDuration)
            {
                if (!_stoped)
                    _stoped = ((SnapToCameraPointsEnemyLogic) _logic).Stoped;
                if (_stoped)
                    base.Shoot();
                return;
            }

            _time = Time.time;
            _timeCooldown = Time.time;
        }

        protected override void ResetActor()
        {
            base.ResetActor();
            
            _stoped = false;
        }
    }
}