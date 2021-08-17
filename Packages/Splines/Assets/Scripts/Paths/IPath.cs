using System;
using UnityEngine;

namespace Splines.Paths
{
    public interface IPath
    {
        event Action OnPathStart;
        event Action OnPathEnd;
        bool Loop { get; }
        void MoveOnPath(float speed, Transform transform, MonoBehaviour mono);
        void Stop();
    }
}