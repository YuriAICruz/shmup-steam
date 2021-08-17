using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class LevelConstructor : IDisposable
    {
        private readonly Level.GenericFactory _genericFactory;
        private readonly CameraManager.Factory _cameraFactory;
        private readonly Player.Factory _playerFactory;
        private readonly Grid _grid;

        public LevelConstructor(Level.GenericFactory genericFactory, CameraManager.Factory cameraFactory,
            Player.Factory playerFactory, Grid grid)
        {
            _genericFactory = genericFactory;
            _cameraFactory = cameraFactory;
            _playerFactory = playerFactory;
            _grid = grid;
            FindRoot();
        }

        private List<GameObject> _construction;

        private Transform _root;
        public Transform Root => _root;

        private void FindRoot()
        {
            if (!_root) return;

            var root = GameObject.Find("Level_Root");

            if (!root)
            {
                root = new GameObject("Level_Root");
            }

            _root = root.transform;
        }

        public void Construct(Level level, Vector3 cameraPosition)
        {
            if (_genericFactory == null)
            {
                Debug.LogError("factory not set, call Level.SetFactory() to set it up");
                return;
            }

            Dispose();

            FindRoot();

            _construction = new List<GameObject>();

            var camera = _cameraFactory.Create(level.levelSettings.cameraMovementType, _root)
                .GetComponent<CameraManager>();
            var player = _playerFactory.Create(level.Player.shipType, _root);

            Setup(level.Player, player.gameObject);

            camera.Setup(cameraPosition);

            _construction.Add(player.gameObject);
            _construction.Add(camera.gameObject);

            Instantiate(level.enemies);
            Instantiate(level.bosses);
            Instantiate(level.walls);
            Instantiate(level.systems);
        }

        private void Instantiate<T>(List<T> assetList) where T : AssetInfo
        {
            for (int i = 0, n = assetList.Count; i < n; i++)
            {
                var obj = _genericFactory.Create(assetList[i].resource,
                    _root,
                    _grid.CellToWorld(assetList[i].position),
                    assetList[i].rotation
                );
                
                Setup(assetList[i], obj);
                _construction.Add(obj);
            }
        }

        private void Setup<T>(T asset, GameObject gameObject) where T : AssetInfo
        {
            gameObject.transform.position = _grid.CellToWorld(asset.position);
            gameObject.transform.rotation = asset.rotation;

            gameObject.GetComponent<IDataInjector>()?.Inject(asset.customData);
        }


        public void Dispose()
        {
            if (_construction == null || _construction.Count == 0)
                return;

            for (int i = 0; i < _construction.Count; i++)
            {
                UnityEngine.Object.Destroy(_construction[i]);
            }
        }
    }
}