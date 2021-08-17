using System;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    public class RotateOverTime : MonoBehaviour
    {
        public Actor actor;

        public float angle = 30;
        public float step = 5;
        public Vector3 axisRotation = new Vector3(0,0,-1);

        private bool _sign;
        private float _angle;
        private Vector3 _forward;
        private Vector3 _eulerAngles;
        private float _initialAngle;

        private void Start()
        {
            actor.Gun.OnShoot += Rotate;

            _forward = transform.forward;
            _eulerAngles = transform.eulerAngles;
            _initialAngle = _eulerAngles.y;
        }

        private void Rotate()
        {
            if (actor.CurrentState == Actor.State.Dead) return;

            _angle += step * (_sign ? 1 : -1);
            
            transform.forward = Quaternion.AngleAxis(_angle, axisRotation) * _forward;

//            transform.up = Vector3.back;

            if (Mathf.Abs(_angle) > angle * 0.5f)
            {
                _angle = angle * 0.5f * (_sign ? 1 : -1);
                _sign = !_sign;
            }
        }

        public void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.white;

            var min = Quaternion.AngleAxis(angle * 0.5f, transform.up) * transform.forward;
            var max = Quaternion.AngleAxis(-angle * 0.5f, transform.up) * transform.forward;

            Gizmos.DrawRay(transform.position, min * 5);
            Gizmos.DrawRay(transform.position, max * 5);

            Gizmos.color = Color.red;
            for (int n = (int)(angle / step), i = -n/2; i < n/2; i++)
            {
                var forward = Quaternion.AngleAxis(i*step, axisRotation) * transform.forward;
            
                Gizmos.DrawRay(transform.position, forward * 15);
            }
#endif
        }
    }
}