using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public abstract class MovingEnemyLogic
    {
        protected float _time;
        protected Vector3 _direction;
        protected Enemy.EnemySettings _settings;

        protected MovingEnemyLogic(Vector3 direction)
        {
            _direction = direction;
        }
        
        public abstract void Calculate(Vector3 position, out Vector3 direction, out Vector3 lookDir);

        public void Move(float deltaTime, Vector3 position, float speed, Enemy.EnemySettings settings, out Vector3 pos, out Vector3 nonDisplaced)
        {
            _settings = settings;
            var displace = _direction * (speed * deltaTime);

            _time += deltaTime;

            var add = 0f;
            switch (settings.movement)
            {
                case Enemy.MovementAddiction.Sine: add = Mathf.Sin(_time * settings.movementSpeed) * settings.movementSize;
                    break;
                case Enemy.MovementAddiction.Cos:
                    add = Mathf.Cos(_time * settings.movementSpeed) * settings.movementSize;
                    break;
                case Enemy.MovementAddiction.YDecrease:
                    _direction.y -= deltaTime * settings.movementSpeed;
                    break;
                case Enemy.MovementAddiction.XDecrease:
                    _direction.x -= deltaTime * settings.movementSpeed;
                    break;
            }

            position += displace;
            nonDisplaced = position;
            pos = position + Vector3.right * add;
        }

        public virtual void Reset(Vector3 direction)
        {
            _direction = direction;
            _time = 0;
        }

        public abstract void Die();
    }
}