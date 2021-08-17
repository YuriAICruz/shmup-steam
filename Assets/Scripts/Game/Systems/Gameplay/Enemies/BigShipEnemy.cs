using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class BigShipEnemy : MovingEnemy
    {
        public class BigShipEnemyLogic : MovingEnemyLogic
        {
            public BigShipEnemyLogic(Vector3 direction) : base(direction)
            {
            }

            public override void Calculate(Vector3 position, out Vector3 direction, out Vector3 lookDir)
            {
                throw new System.NotImplementedException();
            }

            public override void Die()
            {
                throw new System.NotImplementedException();
            }
        }
        
        public override void Spawn(Spawner.SpawnData data, EnemySettings overrideSettings = null, Gun.GunSettings gunOverrideSettings = null)
        {
            _logic.Reset(data.direction.normalized);
            
            base.Spawn(data, overrideSettings, gunOverrideSettings);
        }
        
        protected override void CreateLogic()
        {
            _logic = new BigShipEnemyLogic(_direction);
        }
    }
}