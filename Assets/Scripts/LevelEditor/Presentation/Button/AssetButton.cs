using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Graphene.UiGenerics;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.LevelEditor.Presentation.Button
{
    public class AssetButton : ButtonView
    {
        public class Factory : PlaceholderFactory<AssetButton>
        {
            private readonly DiContainer _container;

            public Factory(DiContainer container)
            {
                _container = container;
            }

            public AssetButton Create(MenuWindow menuType, Transform parent, GameObject target)
            {
                var button = _container.InstantiatePrefabResource($"Editor/Buttons/{menuType}");

                button.transform.SetParent(parent);
                button.transform.localScale = Vector3.one;

                var bt = button.GetComponent<AssetButton>();
                bt.SetTarget(target, menuType);

                return bt;
            }
        }

        private GameObject _target;
        private SignalBus _signalBus;
        private Image _image;
        private MenuWindow _menu;

        [Inject]
        private void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }


        private void SetTarget(GameObject target, MenuWindow menu)
        {
            _target = target;
            _menu = menu;

            _image = GetComponent<Image>();
            var pattern = @"_\d{1,4}";
            var name = Regex.Replace(target.name, pattern, "");
            _image.sprite = Resources.Load<Sprite>($"Editor/Previews/{menu}/{name}");
        }

        protected override void OnClick()
        {
            base.OnClick();

            _signalBus.Fire(new MenuWindowOpen(MenuWindow.None));
            _signalBus.Fire(new BrushSelected(_menu, _target, _image.sprite));
        }
    }
}