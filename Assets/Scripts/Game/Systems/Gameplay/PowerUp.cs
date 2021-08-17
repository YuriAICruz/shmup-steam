using System;
using System.Security.Cryptography.X509Certificates;
using Graphene.Game.Systems.Gameplay.Enemies;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay
{
    public class PowerUp : Collectable
    {
        public class Score
        {
        }

        public class WeaponUpgrade
        {
        }

        public class Bomb
        {
        }

        [Flags]
        public enum Type
        {
            Nothing = 0,
            Score = 1,
            WeaponUpgrade = 2,
            Bomb = 4
        }

        public class Factory : PlaceholderFactory<PowerUp>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public PowerUp Create(Type type)
            {
                var powerUp = _container.InstantiatePrefabResource("PowerUps/" + type).GetComponent<PowerUp>();

                return powerUp;
            }
        }

        public Type type;
        public LayerMask mask;

        [Inject] private SignalBus _signalBus;


        public override void Move()
        {
            var pos = transform.position + _direction * (Time.deltaTime * speed);
            var lastMovement = _cameraManager.LastMovement();

            pos += lastMovement;

            if (Collide())
            {
                pos = transform.position + _direction * (Time.deltaTime * speed);
                //pos += lastMovement;
            }
            else if (!_cameraManager.InBorders(pos))
            {
                var screen = _cameraManager.ScreenPosition(pos);

                var vertical = screen.x >= 0.00f && screen.x <= 1f;

                if (vertical)
                    _direction = Vector3.Reflect(_direction, Vector3.up);
                else
                    _direction = Vector3.Reflect(_direction, Vector3.right);

                pos = transform.position + _direction * (Time.deltaTime * speed);
                pos += lastMovement;
            }

            transform.position = pos;
        }

        private bool Collide()
        {
            if (Physics.Raycast(new Ray(transform.position, _direction), out var hit, 0.6f, mask))
            {
                Debug.DrawRay(transform.position, _direction, Color.green, 5);
                Debug.DrawRay(hit.point, hit.normal, Color.red, 5);
                _direction = Vector3.Reflect(_direction, hit.normal);
                _direction.z = 0;
                _direction.Normalize();
                
                return true;
            }

            return false;
        }

        public override void Collect()
        {
            switch (type)
            {
                case Type.Score:
                    _signalBus.Fire<Score>();
                    break;
                case Type.WeaponUpgrade:
                    _signalBus.Fire<WeaponUpgrade>();
                    break;
                case Type.Bomb:
                    _signalBus.Fire<Bomb>();
                    break;
            }

            Idle = true;
        }
    }
}