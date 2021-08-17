using System;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public enum Ships
    {
        Base = 0
    }
    [Serializable]
    public class LevelSettings
    {
        public Ships playerShip;
        [Space]
        public CameraMovementType cameraMovementType = CameraMovementType.ScrollUp;
        public Vector3 cameraDirection = Vector3.up;
        [Space]
        public bool infinityLives = true;
        public uint maxLives = 3;
        
        public uint maxShipsPool = 24;
        public uint maxPowerUpsPool = 4;
        public uint maxBulletsPool = 2048;
        public float restartDelay = 3;
    }
}