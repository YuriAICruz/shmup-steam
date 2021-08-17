using System;
using System.Collections.Generic;
using Graphene.Game.Systems.Gameplay.Enemies;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class EnemyPool
    {
        private readonly LevelSettings _levelSettings;

        private Dictionary<MovingEnemy.EnemyType, List<IPoolable>> _pool;

        [Inject] private Factory _factory;
        
        private int PoolCount 
        {
            get
            {
                var count = 0;
                foreach (var category in _pool)
                {
                    count += category.Value.Count;
                }

                return count;
            }
        }

        public class Factory : PlaceholderFactory<MovingEnemy>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public MovingEnemy Create(MovingEnemy.EnemyType type, uint variation)
            {
                var me = _container.InstantiatePrefabResource("EnemiesRaw/" + type +"_"+ variation.ToString("00")).GetComponent<MovingEnemy>();
                me.SetType(type);

                return me;
            }
        }

        public EnemyPool(LevelSettings levelSettings)
        {
            _levelSettings = levelSettings;
            _pool = new Dictionary<MovingEnemy.EnemyType, List<IPoolable>>();
        }

        public bool RequestShip(MovingEnemy.EnemyType type, uint variation, out MovingEnemy enemy)
        {
            enemy = null;
            IPoolable p;
            
            if (!_pool.ContainsKey(type))
            {
                _pool.Add(type, new List<IPoolable>());
            }

            if (PoolTools.GetNextInPool(_pool[type], variation, out p))
            {
                enemy = p as MovingEnemy;
                return true;
            }

            if (_pool[type].Count < _levelSettings.maxShipsPool)
            {
                enemy = _factory.Create(type, variation);
                _pool[type].Add(enemy);
                
                return true;
            }
            
            return false;
        }

        public static bool RequestShipLogic(MovingEnemy.EnemyType type, out MovingEnemyLogic logic)
        {
            logic = null;
            
            switch (type)
            {
                case MovingEnemy.EnemyType.Generic:
                    return false;
                case MovingEnemy.EnemyType.Linear:
                    logic = new LinearMovingEnemy.LinearMovingEnemyLogic();
                    return true;
                case MovingEnemy.EnemyType.LinearShooter:
                    logic = new LinearMovingEnemy.LinearMovingEnemyLogic();
                    return true;
                case MovingEnemy.EnemyType.Path:
                    return false;
                case MovingEnemy.EnemyType.SnapToCameraPoints:
                    logic = new SnapToCameraPointsEnemy.SnapToCameraPointsEnemyLogic();
                    return true;
            }
            
            return false;
        }
    }
}