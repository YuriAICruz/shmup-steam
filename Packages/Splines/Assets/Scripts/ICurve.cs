using UnityEngine;

namespace Splines
{
    public enum SplinePointMode
    {
        Free,
        Aligned,
        Mirrored
    }

    public interface ICurve
    {
        void SetMode(int index, SplinePointMode mode);
        SplinePointMode GetMode(int index);

        void SetLoop(bool loop);
        bool GetLoop();

        Vector3 GetPointOnCurve(float value);
        Vector3 GetPointOnCurveTransformed(float value, Transform transform);
        Vector3 GetPoint(int index);
        Vector3 GetPointTransformed(int index, Transform transform);

        Vector3 GetVelocity(float t);
        Vector3 GetDirection(float t);

        void SetPoint(int index, Vector3 pos);
        void SetTransformedPoint(int index, Vector3 pos, Transform transform);
        int Count();
        void Add(Vector3 pos);
        void Add();
        void Add(int index);
        float Distance();
    }
}