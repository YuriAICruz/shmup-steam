using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class Enemy : Actor
    {
        [Serializable]
        public class EnemySettings
        {
            public uint maxHp = 1;
            public float baseSpeed = 3;
            public bool snapToCamera;
            public Vector3 exitDirection = Vector3.down;

            [Header("PowerUps")] [Range(0, 1)] public float powerUpProbability = 0;
            public PowerUp.Type possibilities;

            public MovementAddiction movement;
            public float movementSize = 1;
            public float movementSpeed = 1;
        }

        public enum MovementAddiction
        {
            Nothing = 0,
            Sine = 1,
            Cos = 2,
            YDecrease = 3,
            XDecrease = 4
        }

        [Inject] protected SignalBus _signalBus;
        [Inject] protected PowerUpPool _powerUpPool;
        [Inject] protected Player _player;

        public EnemySettings settings;
        
        private EnemySettings _initialSettings;
        private Gun.GunSettings _initialGunSettings;

        protected EnemySettings InitialSettings
        {
            get
            {
                if (_initialSettings == null)
                {
                    _initialSettings = new EnemySettings();
                    PoolTools.CloneValues(settings, _initialSettings);
                }
                return _initialSettings;
            }
        }        
        
        protected Gun.GunSettings InitialGunSettings
        {
            get
            {
                if (_initialGunSettings == null)
                {
                    _initialGunSettings = new Gun.GunSettings();
                    PoolTools.CloneValues(gunSettings, _initialGunSettings);
                }
                return _initialGunSettings;
            }
        }

        protected override void Awake()
        {
            maxHp = settings.maxHp;
            
            base.Awake();

            _initialSettings = new EnemySettings();
            PoolTools.CloneValues(settings, _initialSettings);
            
            _initialGunSettings = new Gun.GunSettings();
            PoolTools.CloneValues(gunSettings, _initialGunSettings);
            
            gun = _gunFactory.Create(transform, gunSettings, Bullet.Type.Enemy);

            _signalBus.Subscribe<LevelStarted>(ResetActor);
        }

        public override bool DoDamage(int damage = 1)
        {
            var res = base.DoDamage(damage);

            if (res)
            {
                _signalBus.Fire(new BulletKill());
            }
            else
            {
                _signalBus.Fire(new BulletHit());
            }

            return res;
        }

        protected override void Die()
        {
            SpawnCollectable();
            
            base.Die();
        }

        protected override void ResetActor()
        {
            maxHp = settings.maxHp;
            
            base.ResetActor();
        }

        protected virtual void SpawnCollectable()
        {
            if ((int) settings.possibilities == 0)
                return;

            var rnd = UnityEngine.Random.value;

            if (!(rnd < settings.powerUpProbability)) return;

            var vals = new List<int>();
            var i = 0;

            foreach (PowerUp.Type value in Enum.GetValues(typeof(PowerUp.Type)))
            {
                if ((int) (settings.possibilities & value) != 0)
                {
                    vals.Add(i - 1);
                }

                i++;
            }

            var v = Random.Range(0, vals.Count);

            if (_powerUpPool.RequestPowerUp((PowerUp.Type) Mathf.Pow(2, vals[v]), out var powerUp))
            {
                var pos = transform.position;
                powerUp.Spawn(pos, (_player.transform.position - pos).normalized);
                return;
            }
        }
    }
}