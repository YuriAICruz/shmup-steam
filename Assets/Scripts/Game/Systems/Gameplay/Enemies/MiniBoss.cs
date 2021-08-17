using System.Collections.Generic;
using System.ComponentModel;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Graphene.BehaviourTree;
using Graphene.BehaviourTree.Actions;
using Graphene.BehaviourTree.Composites;
using Graphene.BehaviourTree.Conditions;
using Action = System.Action;
using Behaviour = Graphene.BehaviourTree.Behaviour;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class MiniBoss : MovingEnemy
    {
        public class MiniBossEnemyLogic : MovingEnemyLogic
        {
            private readonly float _shootsDuration;
            private readonly EnemySettings _settings;
            private readonly Action _shoot;
            private readonly CameraManager _camera;
            private readonly Player _player;
            private Blackboard _blackboard;
            private Behaviour _tree;
            private Vector3 _look;
            private float _lastTime;

            private int _currentPosition;
            private Vector3 _pos;
            private Vector3[] _positions;


            private Vector2[] _borders = new[]
            {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 1),
            };

            private float _distance;


            enum Ids
            {
                NeedShoot = 1,
                Shoot = 10,
                Move = 15,
                Look = 16,
                CheckCanShoot = 17
            }


            public MiniBossEnemyLogic(float shootsDuration, Vector3 direction, EnemySettings settings, Action shoot, CameraManager camera,
                Player player) :
                base(direction)
            {
                _shootsDuration = shootsDuration;
                _settings = settings;
                _shoot = shoot;
                _camera = camera;
                _player = player;

                _positions = new Vector3[_borders.Length];

                GetPositions();

                _blackboard = new Blackboard();
                _tree = new Behaviour();

                _tree.root = new Priority(new List<Node>
                {
                    new Sequence(new List<Node>()
                    {
                        new CheckBool((int) Ids.NeedShoot),
                        new CallSystemAction((int) Ids.Shoot),
                        new CallSystemAction((int) Ids.Move),
                    }),
                    new Sequence(new List<Node>()
                    {
                        new CallSystemAction((int) Ids.Move),
                        new CallSystemAction((int) Ids.Look),
                        new CallSystemAction((int) Ids.CheckCanShoot),
                    }),
                });

                SetupBlackboard();
            }

            private void GetPositions()
            {
                for (int i = 0; i < _borders.Length; i++)
                {
                    _positions[i] = _camera.ScreenToWorldNormalized(_borders[i], 0.2f);
                }
            }

            private void SetupBlackboard()
            {
                _blackboard.Set((int) Ids.NeedShoot, false, _tree.id);
                _blackboard.Set((int) Ids.Shoot, new System.Action(Shoot), _tree.id);
                _blackboard.Set((int) Ids.Move, new System.Action(Move), _tree.id);
                _blackboard.Set((int) Ids.Look, new System.Action(Look), _tree.id);
                _blackboard.Set((int) Ids.CheckCanShoot, new System.Action(CheckCanShoot), _tree.id);
            }

            public override void Calculate(Vector3 position, out Vector3 direction, out Vector3 lookDir)
            {
                _tree.Tick(null, _blackboard);
                _pos = position;
                direction = _direction;
                lookDir = _look;
            }

            public override void Die()
            {
                _currentPosition = 0;
                _blackboard.Set((int) Ids.NeedShoot, false, _tree.id);
            }


            #region Actions

            void Shoot()
            {
                if (Time.time - _lastTime >= _shootsDuration)
                {
                    _blackboard.Set((int) Ids.NeedShoot, false, _tree.id);
                    GetPositions();
                }

                _direction = Vector3.zero;

                _shoot?.Invoke();
            }

            void Move()
            {
                GetPositions();
                _direction = _positions[_currentPosition] - _pos;
                _distance = _direction.magnitude;
                _direction.Normalize();
            }

            void Look()
            {
                _look = Vector3.Lerp( _look, (_player.transform.position - _pos).normalized, Time.deltaTime * 5);
            }

            void CheckCanShoot()
            {
                if ((_distance * _distance) <= 0.05f)
                {
                    _blackboard.Set((int) Ids.NeedShoot, true, _tree.id);
                    _lastTime = Time.time;
                    _currentPosition = (_currentPosition + 1) % _positions.Length;
                }
            }

            #endregion
        }

        [Space] public float shootsDuration = 2;
        
        public override void Spawn(
            Spawner.SpawnData data,
            EnemySettings overrideSettings = null,
            Gun.GunSettings gunOverrideSettings = null
        )
        {
            _logic.Reset(data.direction.normalized);
            base.Spawn(data, overrideSettings, gunOverrideSettings);
        }

        protected override void Shoot()
        {
        }

        protected override void CreateLogic()
        {
            _logic = new MiniBossEnemyLogic(shootsDuration, _direction, settings, base.Shoot, _cameraManager, _player);
        }
    }
}