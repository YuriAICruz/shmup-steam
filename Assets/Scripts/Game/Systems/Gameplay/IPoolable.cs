namespace Graphene.Game.Systems.Gameplay
{
    public interface IPoolable
    {
        bool Idle { get; }
        uint Variation { get; }
    }
}