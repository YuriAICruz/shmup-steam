using System;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class Shield : MonoBehaviour, IDamageable
    {
        public Action broken;
        public bool DoDamage(int damage = 1)
        {
            if(damage<=1)
                return false;
            
            Break();
            return false;
        }

        private void Break()
        {
            broken?.Invoke();
            gameObject.SetActive(false);
        }

        public void ResetShield()
        {
            gameObject.SetActive(true);
        }
        
    }
}