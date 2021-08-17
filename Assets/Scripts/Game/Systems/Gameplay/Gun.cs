using System;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class Gun : IDisposable
    {
        public enum GunTip
        {
            RootBased = 0,
            Forward = 1,
            Up = 2,
            ToPlayer = 3,
            ToPlayerPrediction = 4,
        }

        [Serializable]
        public class GunSettings
        {
            public Transform[] tips;
            public float interval = 0.4f;
            public int bulletCount;

            [Space] public GunTip tipType;

            [Space] public bool bulletSettingsOverride;
            public Bullet.BulletSettings bulletSettings;

            public void Apply(GunSettings overrideSettings)
            {
                interval = overrideSettings.interval;
                bulletCount = overrideSettings.bulletCount;
                tipType = overrideSettings.tipType;
                if (overrideSettings.bulletSettingsOverride)
                {
                    bulletSettingsOverride = overrideSettings.bulletSettingsOverride;
                    PoolTools.CloneValues(overrideSettings.bulletSettings, bulletSettings);
                }
            }
        }

        [Serializable]
        public class GunUpgrades
        {
            public GunSettings[] gunTierSettings;
        }

        public class Factory : PlaceholderFactory<Gun>
        {
            public Gun Create(Transform root, GunSettings settings, Bullet.Type bulletType)
            {
                var gun = Create();
                gun.Setup(root, settings, bulletType);
                return gun;
            }
        }

        private GunSettings _settings;

        private float _lastShoot;

        [Inject] private BulletPool _pool;
        [Inject] private SignalBus _signalBus;
        [Inject] private LevelManager _levelManager;
        [Inject] private Player _player;

        private Transform _root;
        private Bullet.Type _bulletType;
        private int _bulletsUsed;
        private GunUpgrades _gunUpgrades;
        private int _currentGunUpgrade;
        private float _bulletSpeed;
        public event Action OnShoot;

        public void Setup(Transform root, GunSettings settings, Bullet.Type bulletType)
        {
            _settings = settings;
            _root = root;
            _bulletType = bulletType;
            
            if (!_player && (_settings.tipType == GunTip.ToPlayer || _settings.tipType == GunTip.ToPlayer))
                _settings.tipType = GunTip.RootBased;
        }

        public void SetupForUpgrade(GunUpgrades gunUpgrades)
        {
            _gunUpgrades = gunUpgrades;

            _currentGunUpgrade = 0;
            SetCurrentTier();

            _signalBus.Subscribe<PowerUp.WeaponUpgrade>(Upgrade);
            _signalBus.Subscribe<PlayerDeath>(ResetUpgrades);
        }

        private void ResetUpgrades(PlayerDeath data)
        {
            _currentGunUpgrade = 0;
            SetCurrentTier();
        }

        private void Upgrade(PowerUp.WeaponUpgrade data)
        {
            _currentGunUpgrade = Mathf.Min(_currentGunUpgrade + 1, _gunUpgrades.gunTierSettings.Length - 1);
            SetCurrentTier();
        }

        private void SetCurrentTier()
        {
            _settings = _gunUpgrades.gunTierSettings[_currentGunUpgrade];
        }

        public bool Shoot(int charge)
        {
            if (Time.time - _lastShoot < _settings.interval)
            {
                return false;
            }

            var response = false;

            for (int i = 0; i < _settings.tips.Length; i++)
            {
                if (i > 0 && charge > 0)
                    return response;

                switch (_settings.tipType)
                {
                    case GunTip.RootBased:
                        response |= Shoot(_settings.tips[i].position,
                            (_settings.tips[i].position - _root.position).normalized, charge);
                        break;
                    case GunTip.Forward:
                        response |= Shoot(_settings.tips[i].position, _settings.tips[i].forward, charge);
                        break;
                    case GunTip.Up:
                        response |= Shoot(_settings.tips[i].position, _settings.tips[i].up, charge);
                        break;
                    case GunTip.ToPlayer:
                        if (_player.CurrentState == Actor.State.Dead) return response;
                        
                        response |= Shoot(_settings.tips[i].position,
                            (_player.transform.position - _settings.tips[i].position).normalized, charge);
                        break;
                    case GunTip.ToPlayerPrediction:
                        if (_player.CurrentState == Actor.State.Dead) return response;

                        var pos = _player.transform.position;
                        var distance = pos - _settings.tips[i].position;

                        if (_bulletSpeed <= 0)
                        {
                            GetNextBullet(out var b);
                            _bulletSpeed = b.bulletSettings.speed;
                        }

                        var steps = (distance.magnitude / _bulletSpeed) / Time.deltaTime;

                        pos += _player.MovingDirection * steps; 
                        
                        response |= Shoot(_settings.tips[i].position, (pos - _settings.tips[i].position).normalized, charge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (response)
                OnShoot?.Invoke();

            return response;
        }

        public bool Shoot(Vector3 position, Vector3 dir, int charge)
        {
            if (_bulletsUsed >= _settings.bulletCount)
            {
                return false;
            }

            if (GetNextBullet(out var b))
            {
                _bulletsUsed++;
                
                b.Shoot(position, dir, charge, () => { _bulletsUsed = Mathf.Max(0, _bulletsUsed - 1); });
                _lastShoot = Time.time;

                if (_bulletType == Bullet.Type.Player)
                    _signalBus.Fire(new BulletShoot());

                return true;
            }

            return false;
        }

        private bool GetNextBullet(out Bullet o)
        {
            o = null;

            if (_pool.RequestBullet(_bulletType, out o))
            {
                if (_settings.bulletSettingsOverride)
                    o.SetBulletSettings(_settings.bulletSettings);
                else
                    o.ResetSettings();
                return true;
            }

            // for (int i = 0; i < _settings.bulletCount; i++)
            // {
            //     if (!_bulletsPool[i].Idle)
            //         continue;
            //
            //     o = _bulletsPool[i];
            //     return true;
            // }

            return false;
        }

        public void Dispose()
        {
        }

#if UNITY_EDITOR
        public static void DrawGizmos(Transform root, GunSettings gunSettings)
        {
            Gizmos.color = Color.magenta;

            for (int i = 0; i < gunSettings.tips.Length; i++)
            {
                var dir = Vector3.zero;
                var pos = gunSettings.tips[i].position;
                switch (gunSettings.tipType)
                {
                    case GunTip.RootBased:
                        dir = (pos - root.position).normalized;
                        break;
                    case GunTip.Forward:
                        dir = gunSettings.tips[i].forward;
                        break;
                    case GunTip.Up:
                        dir = gunSettings.tips[i].up;
                        break;
                }

                Gizmos.DrawLine(pos, pos + dir * 2);
            }
        }
#endif
    }
}