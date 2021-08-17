using System.Collections.Generic;
using Graphene.LevelEditor.Presentation.AssetMenus;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.LevelEditor.Presentation
{
    public class PopupManager
    {
        private readonly GraphicRaycaster _raycaster;
        private readonly SignalBus _signalBus;
        private readonly AssetMenu.Factory _factory;
        private Transform _popups;

        private readonly Dictionary<string, AssetMenu> _menus;

        public PopupManager(GraphicRaycaster raycaster, SignalBus signalBus, AssetMenu.Factory factory)
        {
            _raycaster = raycaster;
            _signalBus = signalBus;
            _factory = factory;

            _signalBus.Subscribe<OpenAssetMenu>(OpenAssetMenu);
            _signalBus.Subscribe<CloseAssetMenu>(CloseAssetMenu);

            _popups = new GameObject("Popups").transform;
            _popups.SetParent(_raycaster.transform);
            _popups.localPosition = Vector3.zero;
            _popups.rotation = Quaternion.identity;
            _popups.localScale = Vector3.one;
            
            _menus = new Dictionary<string, AssetMenu>();
        }

        protected virtual void OpenAssetMenu(OpenAssetMenu data)
        {
            var name = data.Target.GameObject.name;

            if (_menus.ContainsKey(name))
            {
                _menus[name].SetTarget(data.Target);
            }
            else
            {
                _menus.Add(name, _factory.Create(data.Target, _popups));
            }

            _menus[name].Open();
        }

        protected virtual void CloseAssetMenu(CloseAssetMenu data)
        {
        }
    }
}