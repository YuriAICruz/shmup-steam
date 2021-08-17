using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Obsolete]
    public class BezierCurve : CurveBase
    {
        public BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            _points = new List<Vector3>();

            _points.Add(p0);
            _points.Add(p1);
            _points.Add(p2);
        }

        public override Vector3 GetPointOnCurve(float value)
        {
            return QuadraticBezier(value);
        }

        private Vector3 QuadraticBezier(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 1);

            for (int i = 1, n = _points.Count; i < limit; i+=2)
            {
                var t = value * (n - 1) - (i - 1);

                t = Mathf.Clamp01(t);

                var oneMinusT = 1 - t;

                pos = oneMinusT * oneMinusT * _points[i - 1] + 2f * oneMinusT * t * _points[i] + t * t * _points[i + 1];
            }

            return pos;
        }
        
        private Vector3 LerpedPoint(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 1);

            for (int i = 1, n = _points.Count; i < limit; i += 1)
            {
                var t = value * (n - 1) - (i - 1);
                pos = Vector3.Lerp(Vector3.Lerp(_points[i - 1], _points[i], t), Vector3.Lerp(_points[i], _points[i + 1], t), t);
            }

            return pos;
        }
        
        public override Vector3 GetVelocity(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 1);

            for (int i = 1, n = _points.Count; i < limit; i++)
            {
                var t = value * (n - 1) - (i - 1);

                t = Mathf.Clamp01(t);

                pos = 
                    2f * (1f - t) * (_points[i] - _points[i-1]) +
                    2f * t * (_points[i+1] - _points[i]);
            }

            return pos;
        }

        public override void Add()
        {
            var last = _points.Count - 1;
            var dir = (_points[last - 1] - _points[last]).normalized;

            for (int i = 0, n = 3; i < n; i++)
            {
                var pos = _points[last] - dir * ((i + 1) / (float)n);
                _points.Add(pos);
            }
        }
    }
}