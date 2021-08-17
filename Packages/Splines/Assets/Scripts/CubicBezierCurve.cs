using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Obsolete]
    public class CubicBezierCurve : CurveBase
    {
        public CubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _points = new List<Vector3>();

            _points.Add(p0);
            _points.Add(p1);
            _points.Add(p2);
            _points.Add(p3);
        }

        public override Vector3 GetPointOnCurve(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 2);

            for (int i = 1, n = _points.Count; i < limit; i++)
            {
                var t = value * (n - 1) - (i - 1);

                t = Mathf.Clamp01(t);

                var oneMinusT = 1 - t;

                pos = 
                    oneMinusT * oneMinusT * oneMinusT * _points[i-1] +
                    3f * oneMinusT * oneMinusT * t * _points[i] +
                    3f * oneMinusT * t * t * _points[i+1] +
                    t * t * t * _points[i+2];
            }

            return pos;
        }
        
        public override Vector3 GetVelocity(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 2);

            for (int i = 1, n = _points.Count; i < limit; i++)
            {
                var t = value * (n - 1) - (i - 1);

                t = Mathf.Clamp01(t);

                var oneMinusT = 1f - t;
                pos = 
                    3f * oneMinusT * oneMinusT * (_points[i] - _points[i-1]) +
                    6f * oneMinusT * t * (_points[i+1] - _points[i]) +
                    3f * t * t * (_points[i+2] - _points[i+1]);
            }

            return pos;
        }
    }
}