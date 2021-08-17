using System;
using System.Collections.Generic;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine.UI;

namespace Graphene.LevelEditor.Presentation.AssetMenus
{
    public class EnemyAssetMenu : AssetMenu
    {
        public Slider id;
        public Slider interval;
        public Slider spawns;
        public Slider spawnsMax;
        public Toggle followPlayer;

        protected override void Save()
        {
            _data = new List<Tuple<string, object>>();

            _data.Add(new Tuple<string, object>(nameof(Spawner.SpawnerSettings.variation), (int) id.value));
            _data.Add(new Tuple<string, object>(nameof(Spawner.SpawnerSettings.interval), interval.value));
            _data.Add(new Tuple<string, object>(nameof(Spawner.SpawnerSettings.maxSpawns), (int) spawns.value));
            _data.Add(new Tuple<string, object>(nameof(Spawner.SpawnerSettings.maxSpawnsAlive), (int) spawnsMax.value));
            _data.Add(new Tuple<string, object>(nameof(Spawner.SpawnerSettings.aimToPlayer), followPlayer.isOn));

            base.Save();
        }
    }
}