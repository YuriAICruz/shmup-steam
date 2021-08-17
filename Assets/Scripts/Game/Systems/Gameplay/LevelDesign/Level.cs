using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    #region AssetInfo

    public class AssetInfo
    {
        public readonly Guid _id;
        public Vector3Int position;
        public Quaternion rotation;
        public List<Tuple<string, object>> customData;

        public string resource;

        public AssetInfo(Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData)
        {
            _id = id;
            this.position = position;
            this.rotation = rotation;
            this.resource = resource;
            this.customData = customData;
        }
    }

    [Serializable]
    public class EnemyInfo : AssetInfo
    {
        public EnemyInfo(Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData) : base(id, position,
            rotation, resource, customData)
        {
        }
    }

    [Serializable]
    public class WallInfo : AssetInfo
    {
        public WallInfo(Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData) : base(id, position,
            rotation, resource, customData)
        {
        }
    }

    [Serializable]
    public class BossInfo : AssetInfo
    {
        public BossInfo(Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData) : base(id, position,
            rotation, resource, customData)
        {
        }
    }

    [Serializable]
    public class SystemInfo : AssetInfo
    {
        public SystemInfo(Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData) : base(id, position,
            rotation, resource, customData)
        {
        }
    }

    [Serializable]
    public class PlayerInfo : AssetInfo
    {
        public Ships shipType;
        public PlayerInfo(Ships shipType, Guid id, Vector3Int position, Quaternion rotation, string resource, List<Tuple<string, object>> customData) : base(id, position,
            rotation, resource, customData)
        {
            this.shipType = shipType;
        }
    }

    #endregion

    [Serializable]
    public class Level
    {
        public class GenericFactory : PlaceholderFactory<GameObject>
        {
            private readonly DiContainer _container;

            public GenericFactory(DiContainer container)
            {
                _container = container;
            }

            public GameObject Create(string path, Transform parent, Vector3 position, Quaternion rotation)
            {
                var go = _container.InstantiatePrefabResource(path,position, rotation, parent);

                return go;
            }
        }

        [Header("Settings")] public LevelSettings levelSettings;

        public SectorSettings initialSectorSettings;

        [Space]
        private PlayerInfo _player;
        
        [Header("Data")] 
        public readonly List<EnemyInfo> enemies;
        public readonly List<WallInfo> walls;
        public readonly List<BossInfo> bosses;
        public readonly List<SystemInfo> systems;

        [JsonIgnore] public PlayerInfo Player => _player;

        public Level()
        {
            levelSettings = new LevelSettings();
            enemies = new List<EnemyInfo>();
            walls = new List<WallInfo>();
            bosses = new List<BossInfo>();
            systems = new List<SystemInfo>();
        }

        private int GetNearbyCount<T>(List<T> assets, Vector3Int center, Vector3Int size) where T : AssetInfo
        {
            return assets.FindAll(x => (x.position - center).magnitude < size.magnitude).Count;
        }

        public void Remove<T>(List<T> list, Guid id) where T : AssetInfo
        {
            var i = list.FindIndex(x => x._id == id);

            if (i < 0) return;

            list.RemoveAt(i);
        }

        public void SetPlayer(Ships shipType, Vector3Int position, Quaternion rotation)
        {
            if (_player != null)
            {
                _player.position = position;
                _player.rotation = rotation;
                return;
            }

            _player = new PlayerInfo(shipType, Guid.NewGuid(), position, rotation,
                $"Player/{levelSettings.playerShip}", null);
        }

        public void Dispose()
        {
        }

        public AssetInfo Get(Vector3Int position)
        {
            AssetInfo asset = enemies.Find(x => x.position == position);
            if (asset!=null)
                return asset;
            
            asset = bosses.Find(x => x.position == position);
            if (asset!=null)
                return asset;
            
            asset = walls.Find(x => x.position == position);
            if (asset!=null)
                return asset;
            
            asset = systems.Find(x => x.position == position);
            if (asset!=null)
                return asset;
            
            
            return null;
        }
    }
}