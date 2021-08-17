using UnityEngine;

namespace Splines
{
    public class SplineWalker : MonoBehaviour
    {
        public Spline Spline;

        public float duration;

        private float _progress;

        private void Awake()
        {
        }

        private void Update()
        {
            _progress += Time.deltaTime / duration;
            if (_progress > 1f)
            {
                _progress = 0f;
            }
            transform.position = Spline.GetPointOnCurve(_progress);
        }
    }
}