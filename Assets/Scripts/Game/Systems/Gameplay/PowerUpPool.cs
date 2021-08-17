using System.Collections.Generic;
using Graphene.Game.Systems.Gameplay.LevelDesign;

namespace Graphene.Game.Systems.Gameplay
{
    public class PowerUpPool
    {
        private readonly LevelSettings _levelSettings;
        private readonly PowerUp.Factory _factory;
        private Dictionary<PowerUp.Type, List<IPoolable>> _pool;

        private int PoolCount(PowerUp.Type type)
        {
            var count = 0;
            foreach (var category in _pool)
            {
                if (category.Key == type)
                    count += category.Value.Count;
            }

            return count;
        }

        public PowerUpPool(LevelSettings levelSettings, PowerUp.Factory factory)
        {
            _levelSettings = levelSettings;
            _factory = factory;

            _pool = new Dictionary<PowerUp.Type, List<IPoolable>>();
        }

        public bool RequestPowerUp(PowerUp.Type type, out PowerUp powerUp)
        {
            powerUp = null;

            if (!_pool.ContainsKey(type))
            {
                _pool.Add(type, new List<IPoolable>());
            }

            if (PoolTools.GetNextInPool(_pool[type], 0, out var p))
            {
                powerUp = p as PowerUp;
                return true;
            }

            if (PoolCount(type) < _levelSettings.maxPowerUpsPool)
            {
                powerUp = _factory.Create(type);
                _pool[type].Add(powerUp);

                return true;
            }

            return false;
        }
    }
}