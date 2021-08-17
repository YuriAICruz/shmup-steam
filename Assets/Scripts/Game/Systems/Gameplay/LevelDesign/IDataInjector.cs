using System;
using System.Collections.Generic;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public interface IDataInjector
    {
        void Inject(List<Tuple<string, object>> customData);
    }
}