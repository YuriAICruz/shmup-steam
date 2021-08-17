using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public class Line : CurveBase
    {
        public Line(Vector3 p0, Vector3 p1)
        {
            _points = new List<Vector3>();

            _points.Add(p0);
            _points.Add(p1);
        }

        public override Vector3 GetPointOnCurve(float value)
        {
            var pos = _points[0];
            
            var limit = (int) Mathf.Ceil(value * (_points.Count-1))+1;
            limit = Mathf.Min(limit, _points.Count);
            
            for (int i = 1, n = _points.Count; i < limit; i++)
            {
                var t = value * (n - 1) - (i - 1);
                pos = Vector3.Lerp(_points[i - 1], _points[i], t);
            }

            return pos;
        }
    }
}