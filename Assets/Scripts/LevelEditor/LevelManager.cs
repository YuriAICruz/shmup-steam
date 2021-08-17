using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Graphene.Game.Installers;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Graphene.LevelEditor
{
    public class LevelManager : ITickable
    {
        private readonly DiContainer _container;
        private readonly LevelAsset.Factory _assetsFactory;
        private readonly Level.GenericFactory _genericFactory;
        private readonly EditorCamera _camera;
        private readonly LevelConstructor _levelConstructor;
        private readonly SignalBus _signal;
        private Level _level;
        private bool _running;
        public Level Level => _level;

        private Queue<Action> _mainThread;
        private PlayerSpawnPoint _player;

        private readonly string _path;

        private Transform _assetsRoot;

        public LevelManager(DiContainer container, LevelAsset.Factory assetsFactory, Level.GenericFactory genericFactory, EditorCamera camera, LevelConstructor levelConstructor)
        {
            _container = container;
            _assetsFactory = assetsFactory;
            _genericFactory = genericFactory;
            _camera = camera;
            _levelConstructor = levelConstructor;
            _mainThread = new Queue<Action>();
            _level = new Level();
            
#if UNITY_EDITOR
            _path = $"{Application.dataPath}/../Levels/";
#else
            _path = $"{Application.persistentDataPath}/Levels/";
#endif
            
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            _player = Object.FindObjectOfType<PlayerSpawnPoint>();
            _level.SetPlayer(Ships.Base,_player.Position, Quaternion.identity);
            
            _assetsRoot = new GameObject("EditorAssets").transform;
        }

        public void Run(Action end = null)
        {
            var path = SaveWithPath();

            var pos = _camera.transform.position;
            
            LevelInstaller.LevelBindings(_container, _level);

            _levelConstructor.Construct(_level, pos);
            
            _assetsRoot.gameObject.SetActive(false);
            
            _running = true;
            var trd = new Thread(() =>
            {
                while (_running)
                {
                }

                _mainThread.Enqueue(() =>
                {
                    _assetsRoot.gameObject.SetActive(true);
                    Load(path);
                });
                _mainThread.Enqueue(end);
            });
            trd.Start();
        }

        public void Save()
        {
            SaveWithPath();
        }

        private string SaveWithPath()
        {
            _level.SetPlayer(_player.shipType, _player.Position, Quaternion.identity);

            var json = JsonConvert.SerializeObject(Level);

            Debug.Log(json);

            var filePath = $"{_path}level_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.lvl";

            File.WriteAllText(filePath, json);

            return filePath;
        }

        public bool Load(string filePath)
        {
            if(File.Exists(filePath))
                return false;

            try
            {
                var json = File.ReadAllText(filePath);
                var lvl = JsonConvert.DeserializeObject<Level>(json);

                _level.Dispose();

                _level = lvl;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }


        public void Tick()
        {
            if (_mainThread.Any())
                _mainThread.Dequeue()?.Invoke();
        }

        public void CreateAsset(Vector3Int pos, MenuWindow menu, GameObject target)
        {
            var id = Guid.NewGuid();
            
            var asset = _assetsFactory.Create(id, pos, menu, target, _assetsRoot);

            var customData = asset.CustomData;
            
            switch (menu)
            {
                case MenuWindow.None:
                case MenuWindow.Settings:
                    return;
                case MenuWindow.Enemies:
                    var boss = target.name.Contains("Boss");

                    if (boss)
                    {
                        _level.bosses.Add(new BossInfo(id, pos, Quaternion.identity,
                            menu + "/" + target.name, customData));
                    }
                    else
                    {
                        _level.enemies.Add(new EnemyInfo(id, pos, Quaternion.identity,
                            menu + "/" + target.name, customData));
                    }

                    break;
                case MenuWindow.Walls:
                    _level.walls.Add(new WallInfo(id, pos, Quaternion.identity,
                        menu + "/" + target.name, customData));
                    break;
                case MenuWindow.Systems:
                    _level.systems.Add(new Game.Systems.Gameplay.LevelDesign.SystemInfo(id, pos, Quaternion.identity,
                        menu + "/" + target.name, customData));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }

        public void DestroyAsset(GameObject reference)
        {
            var asset = reference.GetComponent<LevelAsset>();

            if (!asset) return;

            switch (asset.Menu)
            {
                case MenuWindow.Settings:
                case MenuWindow.None:
                    return;
                case MenuWindow.Enemies:
                    var boss = asset.name.Contains("Boss");

                    if (boss)
                    {
                        _level.Remove(_level.bosses, asset.Id);
                    }
                    else
                    {
                        _level.Remove(_level.enemies, asset.Id);
                    }

                    break;
                case MenuWindow.Walls:
                    _level.Remove(_level.walls, asset.Id);
                    break;
                case MenuWindow.Systems:
                    _level.Remove(_level.systems, asset.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Object.Destroy(reference);
        }

        public bool IsEmpty(Vector3Int position)
        {
            return _level.Get(position) == null;
        }
    }
}