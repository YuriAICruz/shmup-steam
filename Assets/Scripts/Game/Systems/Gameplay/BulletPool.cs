using System.Collections.Generic;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    public class BulletPool
    {
        private readonly LevelSettings _levelSettings;
        private readonly Bullet.Factory _factory;
        private Dictionary<Bullet.Type, List<IPoolable>> _pool;
        
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
        
        public BulletPool(LevelSettings levelSettings, Bullet.Factory factory)
        {
            _levelSettings = levelSettings;
            _factory = factory;
            
            _pool = new Dictionary<Bullet.Type, List<IPoolable>>();
        }
        
        public bool RequestBullet(Bullet.Type type, out Bullet bullet)
        {
            bullet = null;
            if (!_pool.ContainsKey(type))
            {
                _pool.Add(type, new List<IPoolable>());
            }

            if (PoolTools.GetNextInPool(_pool[type], 0, out var p))
            {
                bullet = p as Bullet;
                return true;
            }

            if (PoolCount < _levelSettings.maxBulletsPool)
            {
                bullet = _factory.Create(type);
                _pool[type].Add(bullet);
                
                return true;
            }
            
            return false;
        }
    }
}