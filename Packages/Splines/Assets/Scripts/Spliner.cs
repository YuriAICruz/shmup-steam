using UnityEngine;

namespace Splines
{
    public class Spliner : MonoBehaviour
    {
        public Spline Curve;

        [Range(0, 1)] public float Limit = 1;
        public float Width = 2;

        public Color Color;
    }
}