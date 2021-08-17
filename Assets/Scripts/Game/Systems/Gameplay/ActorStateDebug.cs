using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    [RequireComponent(typeof(Actor))]
    public class ActorStateDebug : MonoBehaviour
    {
        private Actor _actor;
        private Material _material;

        public Renderer reference;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            
            _material = reference.material;
        }

        private void Update()
        {
            SetColorFeedback();
        }

        private void SetColorFeedback()
        {
            switch (_actor.CurrentState)
            {
                case Actor.State.Idle:
                    _material.color = Color.white;
                    break;
                case  Actor.State.Recharging:
                    _material.color = Color.yellow;
                    return;
                case  Actor.State.DodgingHorizontal:
                    _material.color = Color.blue;
                    return;
                case  Actor.State.Dodging:
                    _material.color = new Color(0.4f,0.6f,1,1);
                    return;
                case  Actor.State.Invincible:
                    _material.color = Color.red;
                    return;
            }

            switch (_actor.CurrentWeaponState)
            {
                case  Actor.WeaponState.Holding:
                    _material.color = Color.gray;
                    break;
                case  Actor.WeaponState.Shoot:
                    _material.color = new Color(0.4f, 0.4f, 0.45f, 1f);
                    break;
            }
        }
    }
}