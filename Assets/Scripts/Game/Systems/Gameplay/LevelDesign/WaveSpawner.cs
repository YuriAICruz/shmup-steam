using System.Linq;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class WaveSpawner : Spawner
    {
        public override bool Ended()
        {
            return _spawns >= maxSpawns
                   && _spawnsObjects.All(x => x.Idle);
        }

        public override bool CanEnd()
        {
            return _spawnsObjects.Count(x => !x.Idle) >= maxSpawnsAlive ||
                   _spawns >= maxSpawns;
        }

        protected override void Update()
        {
            var pos = GetPosition();
            if (CanEnd())
            {
                return;
            }

            if (_saw && snapToCamera)
            {
                transform.position += _cameraManager.LastMovement();
                Spawn();
                return;
            }

            if (_cameraManager.InBorders(pos))
            {
                if (!_saw)
                    Saw();
                Spawn();
            }
        }

        public void ResetAndUpgradeWave(int levelUp)
        {
            maxDistance *= 1.2f * levelUp;
            interval *= 0.92f * levelUp;
            maxSpawns += levelUp;
            maxSpawnsAlive += levelUp;
            
            ResetSpawner();
        }
    }
}