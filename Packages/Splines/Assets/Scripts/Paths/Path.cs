using System;
using System.Collections;
using Graphene.Utils;
using UnityEngine;

namespace Splines.Paths
{
    [Serializable]
    public class Path : IPath
    {
        public Path(Path path)
        {
            StartPoint = path.StartPoint;
            EndPoint = path.EndPoint;
            XCurve = path.XCurve;
            YCurve = path.YCurve;
            Ease = path.Ease;
        }

        public Vector2 StartPoint, EndPoint;
        public AnimationCurve XCurve;
        public AnimationCurve YCurve;
        public Equations Ease;
        private Transform _transform;
        private Vector3 _iniPos;

        public event Action OnPathStart, OnPathEnd;
        private Action<float> _evaluate;
        private float _speed;
        private Coroutine _routine;
        private MonoBehaviour _mono;

        public bool _loop;
        public bool Loop
        {
            get { return _loop; }
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

            var dist = (EndPoint - StartPoint).magnitude;

            while ((Time.time - _startTime) / (dist / _speed) < 1)
            {
                tx = RobertPannerEasingLerp.Evaluate((Time.time - _startTime) / (dist / _speed), 1, 0, 1, Ease, XCurve);
                ty = RobertPannerEasingLerp.Evaluate((Time.time - _startTime) / (dist / _speed), 1, 0, 1, Ease, YCurve);

                var pos = new Vector2(
                    StartPoint.x + (EndPoint.x - StartPoint.x) * tx,
                    StartPoint.y + (EndPoint.y - StartPoint.y) * ty
//                    Mathf.Lerp(StartPoint.x, EndPoint.x, tx),
//                    Mathf.Lerp(StartPoint.y, EndPoint.y, ty)
                );

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