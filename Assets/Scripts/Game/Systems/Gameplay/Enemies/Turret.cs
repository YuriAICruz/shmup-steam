using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class Turret : Enemy
    {
        private float _rotation;

        protected override void Awake()
        {
            base.Awake();
            var rdrs = transform.GetComponentsInChildren<MeshRenderer>();
        }

        private void Update()
        {
            gun.Shoot(0);

            _rotation += Time.deltaTime * settings.baseSpeed;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, _rotation));
        }
    }
}