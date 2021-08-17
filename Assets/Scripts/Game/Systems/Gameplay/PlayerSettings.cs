using System;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    [Serializable]
    public class PlayerSettings
    {
        public LayerMask collisionLayer;

        public float turning = 20;
        public float speed = 3;
        public float dodgeLerpSpeed = 4;
        public float spinSpeed = 40;
        public float dodgeSpeedMin = 6;
        public float dodgeSpeedMax = 12;
        public float dodgeMaxDuration = 0.33f;
        public float rechargingDuration = 0.15f;
        public float chargeStepDuration = 0.6f;
        public float invincibilityDuration = 0.4f;
        public float dodgeCooldown = 0.25f;
        public int dashDamage = 5;
    }
}