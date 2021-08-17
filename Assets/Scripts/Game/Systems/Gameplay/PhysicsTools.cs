using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    public class PhysicsTools
    {
        private readonly Transform _transform;
        private readonly Collider[] _collision;

        private Vector3 _lastPosition;
        private int _steps;

        public PhysicsTools(Transform transform)
        {
            _transform = transform;
            _collision = new Collider[2];
            _lastPosition = transform.position;
        }

        public void Reset()
        {
            _steps = 0;
        }

        public bool CheckCollisions(float radius, LayerMask layer, out Transform collision)
        {
            collision = null;
            _steps ++;

            if (_steps <= 2 || (_lastPosition - _transform.position).magnitude > 5000)
            {
                _lastPosition = _transform.position;
                return false;
            }
            
            var pos = _transform.position;

            var dir = pos - _lastPosition;
            var ray = new Ray(_lastPosition, dir);
            
            _lastPosition = pos;

            if (Physics.SphereCast(ray, radius, out var hit, dir.magnitude, layer))
            {
                collision = hit.transform;
                return true;
            }

            var overlaps = Physics.OverlapSphereNonAlloc(ray.origin, radius, _collision, layer);
            for (int i = 0; i < overlaps; i++)
            {
                collision = _collision[i].transform;
                return true;
            }

            return false;
        }
    }
}