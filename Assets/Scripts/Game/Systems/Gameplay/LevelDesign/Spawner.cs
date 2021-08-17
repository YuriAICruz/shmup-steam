using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphene.Game.Systems.Gameplay.Enemies;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class Spawner : MonoBehaviour, IDataInjector
    {
        public class SpawnData
        {
            public Vector3 origin;
            public Vector3 direction;

            public float maxDistance;

            public SpawnData(Vector3 origin, Vector3 direction)
            {
                this.origin = origin;
                this.direction = direction;
            }

            public SpawnData(Vector3 origin, Vector3 direction, float maxDistance)
            {
                this.origin = origin;
                this.direction = direction;
                this.maxDistance = maxDistance;
            }
        }

        public class SpawnerSettings
        {
            [Range(0, 5)] public uint variation;

            [Space] public bool aimToPlayer;
            public bool snapToCamera;

            [Space] [Range(0, 1)] public float cameraHeight = 1;
            public float maxDistance = 1;
            public float interval = 1;
            public int maxSpawns = 3;
            public int maxSpawnsAlive = 3;
        }

        [Inject] protected CameraManager _cameraManager;
        [Inject] protected LevelManager _levelManager;
        [Inject] protected EnemyPool _enemyPool;
        [Inject] protected SignalBus _signalBus;
        [Inject] protected Player _player;

        [Header("Enemy Type")] public MovingEnemy.EnemyType type;
        [Range(0, 5)] public uint variation;


        public Transform tip;

        [Space] public Transform spawnPoint;


        [Space] public SpawnerSettings spawnerSettings;
        [Space] public bool aimToPlayer;
        public bool snapToCamera;
        [Space] [Range(0, 1)] public float cameraHeight = 1;
        public float maxDistance = 1;
        public float interval = 1;
        public int maxSpawns = 3;
        public int maxSpawnsAlive = 3;

        public bool overrideMovingEnemySettings;
        public MovingEnemy.EnemySettings movingEnemySettings;

        public bool overrideMovingEnemyGunSettings;
        public Gun.GunSettings movingEnemyGunSettings;

        [Space] public bool powerUpOnLast;
        public PowerUp.Type possibilities;

        protected float _lastSpawn;
        protected int _spawns;
        protected bool _saw;
        protected int _sector;
        protected Vector3 _initialPosition;

        protected List<MovingEnemy> _spawnsObjects = new List<MovingEnemy>();

        protected Vector3 GetPosition()
        {
            var pos = transform.position;
            pos.x = _cameraManager.transform.position.x;

            return pos;
        }

        protected float GetCameraPosition(Vector3 pos)
        {
            return _cameraManager.ScreenPosition(pos).y;
        }

        public virtual bool Ended()
        {
            return
                _spawns >= maxSpawns
                && _spawnsObjects.All(x => x.Idle);
        }

        public virtual bool CanEnd()
        {
            var pos = GetPosition();
            return
                !_saw && GetCameraPosition(pos) > cameraHeight ||
                _spawns >= maxSpawns ||
                snapToCamera && _saw && _sector != _levelManager.SectorSettings.uid ||
                _spawnsObjects.Count(x => !x.Idle) >= maxSpawnsAlive && _spawns >= maxSpawns ||
                !snapToCamera && _saw && !_cameraManager.InBorders(pos);
        }

        private void Awake()
        {
            _signalBus.Subscribe<LevelStarted>(ResetSpawner);

            _initialPosition = transform.position;
        }


        protected void ResetSpawner()
        {
            _spawnsObjects = new List<MovingEnemy>();
            _spawns = 0;

            _saw = false;
            transform.position = _initialPosition;

            gameObject.SetActive(true);
        }

        protected virtual void Update()
        {
            var pos = GetPosition();
            if (CanEnd())
            {
                if (_saw)
                    EndSpawner();

                return;
            }

            if (_spawnsObjects.Count(x => !x.Idle) >= maxSpawnsAlive)
                return;

            if (_saw && snapToCamera)
            {
                transform.position += _cameraManager.LastMovement();
                Spawn();
                return;
            }

            if (_cameraManager.InBorders(pos))
            {
                if (!_saw)
                    Saw();
                Spawn();
            }
        }

        private void EndSpawner()
        {
            gameObject.SetActive(false);
        }

        protected void Saw()
        {
            _sector = _levelManager.SectorSettings.uid;
            _saw = true;
        }

        protected void Spawn()
        {
            if (Time.time - _lastSpawn < interval)
            {
                return;
            }

            _lastSpawn = Time.time;

            var last = _spawns == maxSpawns - 1;

            if (aimToPlayer)
            {
                tip.position = _player.transform.position;
            }

            var dir = tip.position - spawnPoint.position;
            dir.Normalize();

            var data = new SpawnData(spawnPoint.position, dir, maxDistance);

            if (_enemyPool.RequestShip(type, variation, out var enemy) && enemy.CanSpawn(data))
            {
                var settings = overrideMovingEnemySettings ? movingEnemySettings : null;
                var gun = overrideMovingEnemyGunSettings ? movingEnemyGunSettings : null;

                if (last && powerUpOnLast)
                {
                    var tempSettings = new Enemy.EnemySettings();
                    PoolTools.CloneValues(settings ?? enemy.settings, tempSettings);

                    tempSettings.powerUpProbability = 1;
                    tempSettings.possibilities = possibilities;

                    enemy.Spawn(data, tempSettings, gun);
                }
                else
                {
                    enemy.Spawn(data, settings, gun);
                }

                _spawnsObjects.Add(enemy);

                _spawns++;
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (spawnPoint == null) return;

            Gizmos.color = new Color(0f, 0.65f, 0.25f);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.6f);

            Gizmos.color = new Color(0.6f, 0, 0.75f);
            Gizmos.DrawWireSphere(tip.position, 0.2f);

            var dir = tip.position - spawnPoint.position;

            Gizmos.color = new Color(0.6f, 0, 0.75f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + dir.normalized * 10);

            switch (type)
            {
                case MovingEnemy.EnemyType.Generic:
                    break;
                case MovingEnemy.EnemyType.Linear:
                    break;
                case MovingEnemy.EnemyType.LinearShooter:
                    break;
                case MovingEnemy.EnemyType.Path:
                    break;
                case MovingEnemy.EnemyType.SnapToCameraPoints:
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(spawnPoint.position, maxDistance);
                    break;
            }
#endif
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (spawnPoint == null) return;
            Gizmos.color = new Color(0.9f, 0.5f, 0.1f);
            Vector3 pos, dir, lastPos, look = Vector3.zero;
            pos = lastPos = spawnPoint.position;

            if (!EnemyPool.RequestShipLogic(type, out var enemy))
                return;

            switch (type)
            {
                case MovingEnemy.EnemyType.Linear:
                case MovingEnemy.EnemyType.LinearShooter:
                    enemy.Reset((tip.position - spawnPoint.position).normalized);
                    break;
                case MovingEnemy.EnemyType.SnapToCameraPoints:
                    var points = FindObjectsOfType<CameraFollowPoints>().ToList();
                    ((SnapToCameraPointsEnemy.SnapToCameraPointsEnemyLogic) enemy)
                        .Reset(
                            (tip.position - spawnPoint.position).normalized,
                            points.Find(x => (x.transform.position - transform.position).magnitude < 15),
                            movingEnemySettings.exitDirection);
                    break;
            }

            for (int i = 0, steps = 240; i < steps; i++)
            {
                enemy.Calculate(lastPos, out dir, out look);

                var temp = pos;

                enemy.Move(Time.fixedDeltaTime, lastPos, movingEnemySettings.baseSpeed, movingEnemySettings,
                    out pos, out var nd);

                Gizmos.DrawLine(temp, pos);

                lastPos = nd;
            }
#endif
        }

        public void Inject(List<Tuple<string, object>> customData)
        {
            var type = typeof(Spawner);
            var args = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < args.Length; i++)
            {
                var t = customData.Find(x => args[i].Name == x.Item1);
                if (t == null) continue;

                var value =  Convert.ChangeType(t.Item2, args[i].FieldType);
                args[i].SetValue(this,  value);
            }
        }
    }
}