using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Graphene.Game.Systems.Gameplay.Enemies;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.LevelEditor
{
    [RequireComponent(typeof(Collider))]
    public class LevelAsset : MonoBehaviour, IClickable
    {
        public class Factory : PlaceholderFactory<LevelAsset>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public string GetPath(MenuWindow menuType)
            {
                return $"Editor/LevelAssets/{menuType}";
            }

            public LevelAsset Create(Guid id, Vector3Int position, MenuWindow menuType, GameObject target, Transform parent)
            {
                var asset = _container.InstantiatePrefabResource(GetPath(menuType))
                    .GetComponent<LevelAsset>();

                asset.transform.SetParent(parent);

                asset.SetTarget(id, position, target, menuType);

                return asset;
            }
        }

        private Grid _grid;
        private EditorSettings _settings;

        private SpriteRenderer[] _sprites;
        private GameObject _target;
        private Vector3Int _position;
        public Vector3Int Position => _position;

        private MenuWindow _menu;
        public MenuWindow Menu => _menu;
        private Guid _id;

        public Guid Id => _id;
        public GameObject GameObject => gameObject;
        public virtual bool CanDelete => true;

        private string _name;
        private readonly List<Tuple<string, object>> _customData = new List<Tuple<string, object>>();
        public List<Tuple<string, object>> CustomData => _customData;

        public string AssetName => _name;

        protected virtual void Awake()
        {
            transform.position = _grid.CellToWorld(_grid.WorldToCell(transform.position));
            GetSprites();

            SetColor(_settings.normalColor);
        }

        private void GetSprites()
        {
            if (_sprites != null) return;
            _sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        }

        [Inject]
        private void Initialize(Grid grid, EditorSettings settings)
        {
            _grid = grid;
            _settings = settings;
        }

        private void SetTarget(Guid id, Vector3Int position, GameObject target, MenuWindow menu)
        {
            _id = id;
            _target = target;
            _menu = menu;
            _position = position;

            GetSprites();
            var pattern = @"_\d{1,4}";
            _name = Regex.Replace(target.name, pattern, "");

            SetSprite(Resources.Load<Sprite>($"Editor/Previews/{menu}/{_name}"));

            transform.position = _grid.CellToWorld(_position);
        }

        public void Pick(Vector3Int pos)
        {
            SetColor(_settings.selectedColor);
            transform.position = _grid.CellToWorld(pos);
        }

        public void Release(Vector3Int pos)
        {
            SetColor(_settings.normalColor);
            transform.position = _grid.CellToWorld(pos);
        }

        public void Drag(Vector3 pos)
        {
            transform.position = pos;
        }

        public void Release()
        {
            SetColor(_settings.normalColor);
        }

        public virtual bool HasMenu() => false;

        public void UpdateData(List<Tuple<string, object>> rawData)
        {
            for (int i = 0; i < rawData.Count; i++)
            {
                var index = _customData.FindIndex(x => x.Item1 == rawData[i].Item1);
                if (index >= 0)
                    _customData[index] = rawData[i];
                else
                    _customData.Add(rawData[i]);
            }
        }

        protected virtual void SetSprite(Sprite sprite)
        {
            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].sprite = sprite;
            }
        }

        protected virtual void SetColor(Color color)
        {
            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].color = color;
            }
        }
    }
}