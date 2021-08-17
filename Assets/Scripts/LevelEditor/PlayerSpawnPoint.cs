using Graphene.Game.Systems.Gameplay.LevelDesign;

namespace Graphene.LevelEditor
{
    public class PlayerSpawnPoint : LevelAsset
    {
        public Ships shipType;
        
        public override bool CanDelete => false;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}