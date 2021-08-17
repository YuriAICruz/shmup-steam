using System;
using System.Collections;
using Splines;
using UnityEngine;

namespace Splines.Paths
{
    public class SplinePath : IPath
    {
        private readonly Spline _spline;
        private MonoBehaviour _mono;
        private Coroutine _routine;
        private float _speed;
        private Transform _transform;
        private Vector3 _iniPos;
        public event Action OnPathStart, OnPathEnd;
        
        public bool _loop;
        public bool Loop
        {
            get { return _loop; }
        }

        public SplinePath(Spline spline)
        {
            _spline = spline;
        }
        
        public void MoveOnPath(float speed, Transform transform, MonoBehaviour mono)
        {
            _speed = speed;
            _transform = transform;
            _iniPos = _transform.position;
            
            if (OnPathStart != null) OnPathStart();

            _mono = mono;
            
            _routine = _mono.StartCoroutine(Translate());
        }

        private IEnumerator Translate()
        {
            var _startTime = Time.time;
            var tx = 0f;
            var ty = 0f;

            var dist = _spline.Distance();

            while ((Time.time - _startTime) / (dist / _speed) < 1)
            {
                var t = (Time.time - _startTime) / (dist / _speed);
                
                var pos = _spline.GetPointOnCurve(t);

                _transform.position = _iniPos + new Vector3(pos.x, pos.y);

                yield return null;
            }

            if (_loop)
            {
                MoveOnPath(_speed, _transform, _mono);
            }

            if (OnPathEnd != null) OnPathEnd();
        }

        public void Stop()
        {
            _mono.StopCoroutine(_routine);
        }
    }
}