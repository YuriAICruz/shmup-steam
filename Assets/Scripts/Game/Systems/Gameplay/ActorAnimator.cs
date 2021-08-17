using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    [RequireComponent(typeof(Actor))]
    public class ActorAnimator : MonoBehaviour
    {
        [Space] public Animator animator;
        
        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _actor.stateChange += UpdateState;
            _actor.weaponStateChange += UpdateState;
        }

        private void UpdateState(Actor.WeaponState state)
        {
            animator.SetTrigger(state.ToString());
        }

        private void UpdateState(Actor.State state)
        {
            animator.SetTrigger(state.ToString());
        }
    }
}