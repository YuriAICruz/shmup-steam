using System;
using System.Collections;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    public class Delayer : MonoBehaviour
    {
        public void Delay(Action callback, float delay)
        {
            StartCoroutine(Delay_routine(callback, delay));
        }

        private IEnumerator Delay_routine(Action callback, float delay)
        {
            yield return new WaitForSeconds(delay);

            callback.Invoke();
        }
    }
}