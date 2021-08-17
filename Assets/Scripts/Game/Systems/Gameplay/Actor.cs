using System;
using System.Collections;
using Graphene.Game.Systems.Factories;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class Actor : MonoBehaviour, IDamageable
    {
        public enum State
        {
            Idle = 0,
            Recharging = 2,
            DodgingHorizontal = 5,
            Dodging = 6,
            Invincible = 10,
            Dead = 99,
        }

        public enum WeaponState
        {
            Idle = 0,
            Holding = 1,
            Shoot = 2,
        }

        private State _currentState;
        private WeaponState _currentWeaponState;

        public event Action<State> stateChange;
        public event Action<WeaponState> weaponStateChange;


        public State CurrentState
        {
            get { return _currentState; }
            protected set
            {
                if (_currentState == value) return;
                stateChange?.Invoke(value);
                _currentState = value;
            }
        }

        public WeaponState CurrentWeaponState
        {
            get { return _currentWeaponState; }
            protected set
            {
                if (_currentWeaponState == value) return;
                weaponStateChange?.Invoke(value);
                _currentWeaponState = value;
            }
        }

        public Gun.GunSettings gunSettings;

        protected Gun gun;

        public Gun Gun => gun;
        
        public float Hp => _hp;

        [Space]
        public uint maxHp;

        protected int _hp;

        [Inject] protected Explosion.Factory _explosions;
        [Inject] protected Delayer _delayer;
        [Inject] protected CameraManager _cameraManager;
        [Inject] protected Gun.Factory _gunFactory;

        protected Vector3 _initialPosition;

        protected virtual void Awake()
        {
            _initialPosition = transform.position;

            ResetActor();
        }

        protected virtual void Start()
        {
            
        }

        public virtual bool DoDamage(int damage = 1)
        {
            _hp -= damage;

            if (_hp <= 0)
            {
                Die();
                return true;
            }

            return false;
        }

        protected virtual void Die()
        {
            _explosions.Create(transform.position);
            transform.position = Vector3.one * -999;
            //ResetActor();
        }

        protected virtual void ResetActor()
        {
            transform.position = _initialPosition;
            _hp = (int) maxHp;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gun.DrawGizmos(transform, gunSettings);
#endif
        }
    }
}