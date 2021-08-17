using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public abstract class CurveBase : ICurve
    {
        [SerializeField]
        protected bool Loop;
        
        [SerializeField]
        protected List<Vector3> _points;

        public bool IsInitialized;

        public virtual void SetMode(int index, SplinePointMode mode)
        {
            throw new NotImplementedException();
        }

        public virtual SplinePointMode GetMode(int index)
        {
            throw new NotImplementedException();
        }

        public virtual void SetLoop(bool loop)
        {
            Loop = loop;
        }

        public bool GetLoop()
        {
            return Loop;
        }

        public virtual Vector3 GetPointOnCurve(float value)
        {
            throw new NotImplementedException();
        }

        public Vector3 GetPointOnCurveTransformed(float value, Transform transform)
        {
            return transform.TransformPoint(GetPointOnCurve(value));
        }

        public Vector3 GetPoint(int index)
        {
            if (index >= _points.Count) return Vector3.zero;
            return _points[index];
        }

        public Vector3 GetPointTransformed(int index, Transform transform)
        {
            if (index >= _points.Count) return Vector3.zero;

            return transform.TransformPoint(GetPoint(index));
        }

        public virtual Vector3 GetVelocity(float value)
        {
            return Vector3.zero;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public virtual void SetPoint(int index, Vector3 pos)
        {
            if (index >= _points.Count) return;
            _points[index] = pos;
        }

        public void SetTransformedPoint(int index, Vector3 pos, Transform transform)
        {
            if (index >= _points.Count) return;
            SetPoint(index, transform.InverseTransformPoint(pos));
        }

        public int Count()
        {
            if (_points == null)
            {
                _points = new List<Vector3>();
            }

            return _points.Count;
        }

        public void Add(Vector3 pos)
        {
            _points.Add(pos);
        }

        public virtual void Add()
        {
            var last = _points.Count - 1;
            _points.Add(_points[last] - (_points[last - 1] - _points[last]).normalized);
        }

        public virtual void Add(int index)
        {
            if (index >= _points.Count-1) return;
            
            var last = index;
            _points.Insert(index, _points[last] - (_points[last - 1] - _points[last]).normalized);
        }
        

        public virtual float Distance()
        {
            var dist = 0f;
            for (int i = 1; i < _points.Count; i++)
            {
                dist += Vector3.Distance(_points[i - 1], _points[i]);
            }

            return dist;
        }
    }
}