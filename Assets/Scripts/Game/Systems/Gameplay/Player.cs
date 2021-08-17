using System;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using Graphene.Game.Systems.Input;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class Player : Actor
    {
        public class Factory:PlaceholderFactory<Player>
        {
            private readonly DiContainer _container;
            private Player _player;
            public Factory(DiContainer container)
            {
                _container = container;
            }
            
            public Player Create(Ships type, Transform root)
            {
                if (_player) return _player;
                
                var player = _container.InstantiatePrefabResource($"Player/{type}").GetComponent<Player>();
                
                player.transform.SetParent(root);

                _player = player;
                _container.Bind<Player>().FromInstance(_player);
                
                return player;
            }
        }
        public event Action OnHit;
        public event Action OnDie;

        public Gun.GunUpgrades gunUpgrades;

        public float Dodging => _dodging;
        public int HoldingStep => (int) Mathf.Min((int) _holding / playerSettings.chargeStepDuration, 4);

        public PlayerSettings playerSettings;

        [Space] public PhysicsTools _physics;

        public SkinnedMeshRenderer meshRenderer;

        private Vector2 _move;
        private Vector2 _look;
        private float _holding;
        private float _dodging;
        private float _lastDodge;

        private Vector2 _dodgeDirection;
        private float _recharging;
        private float _bank;
        private SphereCollider _collider;

        [Inject] private LevelManager _levelManager;
        [Inject] private SignalBus _signalBus;
        [Inject] private InputSettings _inputSettings;

        public Vector3 MovingDirection
        {
            get { return (Vector3) _move * (playerSettings.speed * Time.deltaTime); }
        }

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();

            BecomeInvincible();

            _collider = GetComponent<SphereCollider>();
            _physics = new PhysicsTools(transform);

            _signalBus.Subscribe<InputManager.InputDownEvent>(OnDown);
            _signalBus.Subscribe<InputManager.InputUpEvent>(OnUp);
            _signalBus.Subscribe<InputManager.InputConstantEvent>(InputUpdate);
            _signalBus.Subscribe<InputManager.InputAxisEvent>(AxisUpdate);

            _signalBus.Subscribe<LevelStarted>(ResetPlayer);
        }

        private void OnDestroy()
        {
            gun?.Dispose();
        }

        protected override void Start()
        {
            if (gun == null)
            {
                gun = _gunFactory.Create(transform, gunSettings, Bullet.Type.Player);
                gun.SetupForUpgrade(gunUpgrades);
            }

            _recharging = playerSettings.rechargingDuration;
            _levelManager.StartLevel();
        }

        private void Update()
        {
            ActorUpdate();
        }

        private void FixedUpdate()
        {
            if (CurrentState == State.Dead) return;

            if (_physics.CheckCollisions(_collider.radius, playerSettings.collisionLayer, out var hit))
            {
                Collide(hit.transform);
            }
        }

        #endregion

        #region Inputs

        private void AxisUpdate(InputManager.InputAxisEvent input)
        {
            switch (input.index)
            {
                case 0:
                    _move = input.axis;
                    break;
                case 1:
                    if (_inputSettings.Mouse)
                    {
                        _look = _cameraManager.GetDirectionToMouse(transform.position);
                    }
                    else
                    {
                        _look = input.axis;
                    }

                    break;
            }
        }

        private void InputUpdate(InputManager.InputConstantEvent input)
        {
            if (input.action == InputSettings.Input.Action.RapidFire && input.down &&
                CurrentWeaponState != WeaponState.Holding)
                RapidShoot();
        }

        private void OnUp(InputManager.InputUpEvent input)
        {
            if (input.action == InputSettings.Input.Action.Shoot)
            {
                Shoot();
            }

            if (input.action == InputSettings.Input.Action.Dodge)
            {
                DodgeRelease();
            }
        }

        private void OnDown(InputManager.InputDownEvent input)
        {
            if (input.action == InputSettings.Input.Action.Shoot)
            {
                HoldShoot();
            }

            if (input.action == InputSettings.Input.Action.Dodge)
            {
                Dodge();
            }
        }

        #endregion

        #region Actions

        private void Move(Vector2 move, float speed)
        {
            var pos = transform.position + (Vector3) move * (speed * Time.deltaTime);

            pos += _cameraManager.LastMovement();

            if (!_cameraManager.InBorders(pos))
            {
                if (!_cameraManager.InBorders(pos, CameraManager.BorderType.OffsetOut))
                {
                    pos = _cameraManager.ClosestPoint(pos, CameraManager.BorderType.SafeArea);
                }
                else
                {
                    pos = _cameraManager.ClosestPoint(pos);
                }
            }

            transform.position = pos;
        }

        private void DodgeRelease()
        {
            if (CurrentState != State.Dodging && CurrentState != State.DodgingHorizontal) return;

            Recharging();

            meshRenderer.SetBlendShapeWeight(0, 0);
        }

        private void Dodge()
        {
            if(CurrentState == State.Dead) return;
            
            if (Time.time - _lastDodge < playerSettings.dodgeCooldown)
                return;

            _lastDodge = Time.time;

            _dodging = 0;

            if (_move.magnitude <= 0)
            {
                _dodgeDirection = -transform.forward;
                return;
            }

            _dodgeDirection = _move;


            if (_inputSettings.Mouse)
            {
                _look = _dodgeDirection;
                Turn();
            }
            
            if (((Vector2) transform.forward - _move).magnitude < 0.6f)
            {
                CurrentState = State.Dodging;
            }
            else
            {
                CurrentState = State.DodgingHorizontal;
            }

            meshRenderer.SetBlendShapeWeight(0, 0);
        }

        private void RapidShoot()
        {
            CurrentWeaponState = WeaponState.Holding;
            _holding = 0;
            Shoot();
        }

        private void Shoot()
        {
            if (CurrentWeaponState != WeaponState.Holding) return;

            if (gun.Shoot(HoldingStep))
            {
                CurrentWeaponState = WeaponState.Shoot;

                if (_holding >= playerSettings.chargeStepDuration)
                {
                    _delayer.Delay(() =>
                    {
                        CurrentState = State.Recharging;
                        CurrentWeaponState = WeaponState.Idle;
                    }, playerSettings.rechargingDuration);
                }
                else
                {
                    _delayer.Delay(() =>
                    {
                        if (CurrentWeaponState != WeaponState.Holding)
                            CurrentWeaponState = WeaponState.Idle;
                    }, 0.4f);
                }

                return;
            }

            if (_holding >= playerSettings.chargeStepDuration)
            {
                CurrentState = State.Recharging;
                CurrentWeaponState = WeaponState.Idle;
            }
            else
            {
                CurrentWeaponState = WeaponState.Idle;
            }
        }

        private void HoldShoot()
        {
            if (CurrentState == State.Invincible) return;

            CurrentWeaponState = WeaponState.Holding;

            _holding = 0;
        }

        #endregion

        private void ActorUpdate()
        {
            if (CurrentWeaponState == WeaponState.Holding)
            {
                _holding += Time.deltaTime;
            }

            switch (CurrentState)
            {
                case State.Invincible:
                case State.Idle:
                    Move(_move, playerSettings.speed);
                    Turn();
                    break;
                case State.DodgingHorizontal:
                case State.Dodging:
                    _dodging += Time.deltaTime;

                    if (_dodging >= playerSettings.dodgeMaxDuration)
                        DodgeRelease();

                    _dodgeDirection = Vector3.Lerp(_dodgeDirection, _move,
                        Time.deltaTime * playerSettings.dodgeLerpSpeed);

                    Move(_dodgeDirection,
                        Mathf.Lerp(playerSettings.dodgeSpeedMax, playerSettings.dodgeSpeedMin,
                            _dodging / playerSettings.dodgeMaxDuration));
                    //Spin(_move.magnitude);
                    break;
                case State.Recharging:
                    Move(Vector2.zero, 0);
                    _recharging += Time.deltaTime;
                    if (_recharging >= playerSettings.rechargingDuration)
                        Recharged();
                    break;
            }
        }

        private void Collide(Transform hit)
        {
            if (CurrentState == State.DodgingHorizontal)
                CurrentState = State.Idle;

            var collectable = hit.GetComponent<ICollectable>();
            if (collectable != null)
            {
                Collect(collectable);
                return;
            }

            var damageable = hit.GetComponent<IDamageable>();

            if (damageable != null)
            {
                if (CurrentState == State.Dodging)
                {
                    if (damageable.DoDamage(playerSettings.dashDamage))
                    {
                        _signalBus.Fire(new DashKill());
                    }
                    else
                    {
                        CurrentState = State.Idle;
                    }
                }
                else
                {
                    damageable.DoDamage(_hp * 2);
                }
            }
            else
            {
                CurrentState = State.Idle;
            }

            DoDamage(_hp);
        }

        private void Collect(ICollectable collectable)
        {
            collectable.Collect();
        }

        private void Spin(float moveDir)
        {
            _bank = Mathf.Lerp(_bank, _bank - moveDir * playerSettings.turning,
                Time.deltaTime * playerSettings.spinSpeed);

            transform.rotation = Quaternion.Euler(new Vector3(-90, 0, _bank));
        }

        private void Turn()
        {
            //_bank = Mathf.Lerp(_bank, -move.x * playerSettings.turning, Time.deltaTime * speed * 5);

            //transform.rotation = Quaternion.Euler(new Vector3(-90, 0, _bank));

            if (_look.sqrMagnitude > 0.1f)
            {
                var rot = Mathf.Rad2Deg * Mathf.Atan2(-_look.y, -_look.x);

                //transform.forward = new Vector3(_look.x, _look.y, 0);

                transform.rotation = Quaternion.Euler(new Vector3(rot, -90, 90));
                //transform.up = Vector3.back; 
            }
        }

        private void Recharged()
        {
            CurrentState = State.Idle;
        }

        private void Recharging()
        {
            if (_dodging >= playerSettings.dodgeMaxDuration / 3f)
            {
                _recharging = 0;
            }

            CurrentState = State.Recharging;
        }

        public override bool DoDamage(int damage = 1)
        {
            if (CurrentState == State.Invincible ||
                CurrentState == State.Dodging ||
                CurrentState == State.DodgingHorizontal)
            {
                if (CurrentState == State.Dodging)
                    _signalBus.Fire(new DashReflect());

                return false;
            }

            if (CurrentWeaponState == WeaponState.Holding)
                CurrentWeaponState = WeaponState.Idle;


            _signalBus.Fire(new PlayerHit());

            _hp -= damage;

            CurrentState = State.Invincible;

            BecomeInvincible();

            if (_hp <= 0)
            {
                OnDie?.Invoke();
                Die();
                return true;
            }

            OnHit?.Invoke();
            return false;
        }

        private void BecomeInvincible()
        {
            _delayer.Delay(() =>
            {
                if (CurrentState == State.Invincible)
                {
                    CurrentState = State.Idle;
                }
            }, playerSettings.invincibilityDuration);
        }

        protected override void Die()
        {
            _physics.Reset();
            _explosions.Create(transform.position);
            CurrentState = State.Dead;

            transform.position = Vector3.one * 9999;

            _signalBus.Fire(new PlayerDeath());
        }

        protected virtual void ResetPlayer()
        {
            _physics.Reset();

            //_cameraManager.ResetCamera();

            _hp = (int) maxHp;

            transform.position = _initialPosition;

            CurrentState = State.Idle;
        }
    }
}