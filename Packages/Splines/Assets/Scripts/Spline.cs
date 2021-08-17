using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public class Spline : CurveBase
    {
        [SerializeField] private List<SplinePointMode> _modes;

        public Spline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _points = new List<Vector3>();

            _points.Add(p0);
            _points.Add(p1);
            _points.Add(p2);
            _points.Add(p3);

            _modes = new List<SplinePointMode>();
            _modes.Add(SplinePointMode.Aligned);
            _modes.Add(SplinePointMode.Aligned);

            IsInitialized = true;
        }

        public Spline(Spline spline)
        {
            _points = new List<Vector3>(spline._points);
            _modes = new List<SplinePointMode>(spline._modes);
            Loop = spline.Loop;

            IsInitialized = true;
        }

        public override void SetMode(int index, SplinePointMode mode)
        {
            var modeIndex = (index + 1) / 3;
            if (modeIndex >= _modes.Count) return;

            _modes[modeIndex] = mode;

            if (Loop)
            {
                if (modeIndex == 0)
                {
                    _modes[_modes.Count - 1] = mode;
                }
                else if (modeIndex == _modes.Count - 1)
                {
                    _modes[0] = mode;
                }
            }

            EnforceMode(index);
        }

        public override SplinePointMode GetMode(int index)
        {
            var modeIndex = (index + 1) / 3;
            if (modeIndex >= _modes.Count) return SplinePointMode.Aligned;

            return _modes[modeIndex];
        }

        public override void SetLoop(bool loop)
        {
            if (loop)
            {
                _modes[_modes.Count - 1] = _modes[0];
                SetPoint(0, _points[0]);
            }

            base.SetLoop(loop);
        }

        public override Vector3 GetPointOnCurve(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            int i;
            var t = value;
            var curves = ((_points.Count - 1) / 3);
            if (value >= 1)
            {
                t = 1;
                i = _points.Count - 4;
            }
            else
            {
                t = (value * curves);
                i = (int) t;
                t -= i;
                i *= 3;
            }

            t = Mathf.Clamp01(t);

            var oneMinusT = 1 - t;

            pos =
                oneMinusT * oneMinusT * oneMinusT * _points[i] +
                3f * oneMinusT * oneMinusT * t * _points[i + 1] +
                3f * oneMinusT * t * t * _points[i + 2] +
                t * t * t * _points[i + 3];

            return pos;
        }

        public override Vector3 GetVelocity(float value)
        {
            value = Mathf.Clamp01(value);
            var pos = _points[0];

            var limit = (int) Mathf.Ceil(value * (_points.Count - 1)) + 1;
            limit = Mathf.Min(limit, _points.Count - 2);

            for (int i = 1, n = _points.Count; i < limit; i += 3)
            {
                var t = value * (n - 1) - (i - 1);

                t = Mathf.Clamp01(t);

                var oneMinusT = 1f - t;
                pos =
                    3f * oneMinusT * oneMinusT * (_points[i] - _points[i - 1]) +
                    6f * oneMinusT * t * (_points[i + 1] - _points[i]) +
                    3f * t * t * (_points[i + 2] - _points[i + 1]);
            }

            return pos;
        }

        public override void SetPoint(int index, Vector3 pos)
        {
            if (index % 3 == 0)
            {
                var delta = pos - _points[index];
                var count = _points.Count;

                if (Loop)
                {
                    if (index == 0)
                    {
                        _points[1] += delta;
                        _points[count - 2] += delta;
                        _points[count - 1] = pos;
                    }
                    else if (index == count - 1)
                    {
                        _points[0] = pos;
                        _points[1] += delta;
                        _points[index - 1] += delta;
                    }
                    else
                    {
                        _points[index - 1] += delta;
                        _points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _points[index - 1] += delta;
                    }
                    if (index + 1 < count)
                    {
                        _points[index + 1] += delta;
                    }
                }
            }

            base.SetPoint(index, pos);

            EnforceMode(index);
        }

        private void EnforceMode(int index)
        {
            var modeIndex = (index + 1) / 3;
            if (modeIndex >= _modes.Count) return;
            var mode = _modes[modeIndex];

            if (mode == SplinePointMode.Free || !Loop && (modeIndex == 0 || modeIndex >= _modes.Count - 1)) return;

            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = _points.Count - 2;
                }
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= _points.Count)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= _points.Count)
                {
                    fixedIndex = 1;
                }
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = _points.Count - 2;
                }
            }

            var middle = _points[middleIndex];
            var enforcedTangent = middle - _points[fixedIndex];
            if (mode == SplinePointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, _points[enforcedIndex]);
            }
            _points[enforcedIndex] = middle + enforcedTangent;
        }

        public override void Add()
        {
            if (_points == null)
            {
                _points = new List<Vector3>();
            }
            if (_modes == null)
            {
                _modes = new List<SplinePointMode>();
            }

            var last = _points.Count - 1;
            var n = 3;
            Vector3 dir;
            if (last < 0)
            {
                n += 1;
                dir = Vector3.left*3;
            }
            else
            {
                dir = (_points[last - 1] - _points[last]).normalized;
            }

            for (var i = 0; i < n; i++)
            {
                var pos = dir * ((i + 1) / (float) n);
                if (last >= 0)
                    pos = _points[last] - dir * ((i + 1) / (float) n);
                _points.Add(pos);
            }

            _modes.Add(SplinePointMode.Aligned);

            if (last < 0)
                _modes.Add(SplinePointMode.Aligned);

            EnforceMode(_points.Count - 4);

            if (Loop && last > 0)
            {
                _points[_points.Count - 1] = _points[0];
                _modes[_modes.Count - 1] = _modes[0];
                EnforceMode(0);
            }
        }

        public override void Add(int index)
        {
            if (index >= _points.Count - 1) return;

            var last = index;
            var dir = Vector3.one;

            if (index > 0)
                dir = (_points[last - 1] - _points[last]).normalized;

            for (int i = 0, n = 3; i < n; i++)
            {
                var pos = _points[last] - dir * ((i + 1) / (float) n);
                _points.Insert(index + i + 1, pos);
            }

            _modes.Add(SplinePointMode.Aligned);
            EnforceMode(_points.Count - 1);
        }
    }
}