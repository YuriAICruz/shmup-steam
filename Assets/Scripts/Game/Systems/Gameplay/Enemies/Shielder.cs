namespace Graphene.Game.Systems.Gameplay.Enemies
{
    public class Shielder : Enemy
    {
        public Shield shield;

        protected override void ResetActor()
        {
            base.ResetActor();
            shield.ResetShield();
        }

        private void Update()
        {
            gun.Shoot( 0);
        }
    }
}