using System;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class LinearMovingEnemy : MovingEnemy
    {
        public class LinearMovingEnemyLogic : MovingEnemyLogic
        {
            public LinearMovingEnemyLogic() : base(Vector3.zero)
            {
            }

            public LinearMovingEnemyLogic(Vector3 direction) : base(direction)
            {
                _direction = direction;
            }

            public override void Calculate(Vector3 position, out Vector3 direction, out Vector3 lookDir)
            {
                direction = _direction;
                lookDir = _direction;
            }

            public override void Die()
            {
            }
        }

        public override void Spawn(Spawner.SpawnData data, EnemySettings overrideSettings = null,
            Gun.GunSettings gunOverrideSettings = null)
        {
            _logic.Reset(data.direction.normalized);

            base.Spawn(data, overrideSettings, gunOverrideSettings);
        }

        protected override void CreateLogic()
        {
            _logic = new LinearMovingEnemyLogic(_direction);
        }
    }
}