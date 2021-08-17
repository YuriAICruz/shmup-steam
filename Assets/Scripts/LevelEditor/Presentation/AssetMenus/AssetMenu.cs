using System;
using System.Collections.Generic;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using Graphene.UiGenerics;
using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor.Presentation.AssetMenus
{
    [RequireComponent(typeof(CanvasGroupView))]
    public class AssetMenu : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<AssetMenu>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public AssetMenu Create(IClickable clickable, Transform parent)
            {
                var asset = _container.InstantiatePrefabResource($"Editor/AssetMenus/{clickable.AssetName}").GetComponent<AssetMenu>();
                
                asset.transform.SetParent(parent);
                asset.transform.localScale = Vector3.one;

                asset.SetTarget(clickable);
                
                return asset.GetComponent<AssetMenu>();
            }
        }

        public UnityEngine.UI.Button closeButton;
        public UnityEngine.UI.Button saveButton;
        
        private CanvasGroupView _cv;
        private IClickable _target;
        protected List<Tuple<string, object>> _data;

        protected virtual void Awake()
        {
            _cv = GetComponent<CanvasGroupView>();
            
            closeButton.onClick.AddListener(Close);
            saveButton.onClick.AddListener(Save);
        }

        public void SetTarget(IClickable target)
        {
            _target = target;
        }

        public void Open()
        {
            transform.position = _target.GameObject.transform.position + new Vector3(1,1,0);
            _cv.Show();
        }

        public void Close()
        {
            _cv.Hide();
        }

        protected virtual void Save()
        {
            _target.UpdateData(_data);
        }
    }
}