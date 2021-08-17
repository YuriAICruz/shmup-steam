using System;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    [Serializable]
    public class SectorSettings
    {
        [HideInInspector]
        public int uid;
        [HideInInspector]
        public Vector3 position;
        
        public float cameraSpeed = 2;

        public CameraFollowPoints[] cameraFollowPoints;
        
        [Space]
        public bool waitToEnd;
        public Spawner spawner;
        public float endCameraSpeed = 2;

        public bool GetNextCloserCameraPoint(Vector3 position, float maxDistance, out CameraFollowPoints point)
        {
            point = null;
            for (int i = 0; i < cameraFollowPoints.Length; i++)
            {
                if (!cameraFollowPoints[i].IsAvailable)
                    continue;

                if((position - cameraFollowPoints[i].transform.position).magnitude > maxDistance)
                    continue;
                
                point = cameraFollowPoints[i];
                return true;
            }

            return false;
        }
        
        public bool GetNextCameraPoint(out CameraFollowPoints point)
        {
            point = null;
            for (int i = 0; i < cameraFollowPoints.Length; i++)
            {
                if (!cameraFollowPoints[i].IsAvailable)
                    continue;

                point = cameraFollowPoints[i];
                return true;
            }

            return false;
        }
    }
}