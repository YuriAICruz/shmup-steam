using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    [RequireComponent(typeof(Player))]
    public class PlayerFx : MonoBehaviour
    {
        public ParticleSystem rechargingParticles;
        public ParticleSystem holdingParticles;

        private Player _player;

        private ParticleSystem.MainModule _holdingParticlesMain;
        private float _holdingParticlesLife;

        private void Awake()
        {
            _player = GetComponent<Player>();

            _holdingParticlesMain = holdingParticles.main;
            _holdingParticlesLife = _holdingParticlesMain.startLifetime.constant;
        }

        private void Update()
        {
            if (_player.CurrentWeaponState == Actor.WeaponState.Holding)
            {
                if (!holdingParticles.isPlaying)
                {
                    _holdingParticlesMain.startLifetime = _holdingParticlesLife;
                    holdingParticles.Play();
                }
                else
                {
                    _holdingParticlesMain.startLifetime = _holdingParticlesLife + _player.HoldingStep * 0.2f;
                }
            }
            else
            {
                if (holdingParticles.isPlaying)
                    holdingParticles.Stop();
            }

            switch (_player.CurrentState)
            {
                case Actor.State.Idle:
                    break;
                case Actor.State.Recharging:
                    if (!rechargingParticles.isPlaying)
                        rechargingParticles.Play();
                    break;
                case Actor.State.DodgingHorizontal:
                    break;
                case Actor.State.Dodging:
                    break;
                case Actor.State.Invincible:
                    break;
                case Actor.State.Dead:
                    break;
            }
        }
    }
}