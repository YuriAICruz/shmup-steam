using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay
{
    public static class PoolTools
    {
        public static bool GetNextInPool(List<IPoolable> pools, uint variation, out IPoolable item)
        {
            item = pools.Find(x => x.Idle && x.Variation == variation);

            return item != null;
        }
        
        public static void CloneValues(object baseObject, object target)
        {
            if(baseObject == null) return;
            var parms = target.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public);
            for (int i = 0; i < parms.Length; i++)
            {
                parms[i].SetValue(target, parms[i].GetValue(baseObject));
            }
        }
    }
}