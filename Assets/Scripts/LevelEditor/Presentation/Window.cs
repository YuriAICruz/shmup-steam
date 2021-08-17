using System;
using System.Linq;
using System.Runtime.InteropServices;
using Graphene.Game.Systems.Gameplay.Enemies;
using Graphene.LevelEditor.Presentation.Button;
using Graphene.UiGenerics;
using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor.Presentation
{
    [RequireComponent(typeof(CanvasGroupView))]
    public class Window : MonoBehaviour
    {
        public MenuWindow menu;
        public Transform content;

        private SignalBus _signalBus;
        private CanvasGroupView _cv;
        private AssetButton.Factory _buttonFactory;

        private void Awake()
        {
            _cv = GetComponent<CanvasGroupView>();
            _cv.Hide();

            _signalBus.Subscribe<MenuWindowOpen>(UpdateWindow);

            Populate();
        }

        [Inject]
        private void Initialize(SignalBus signalBus, AssetButton.Factory buttonFactory)
        {
            _buttonFactory = buttonFactory;
            _signalBus = signalBus;
        }

        private void Populate()
        {
            if (menu == MenuWindow.Settings || menu == MenuWindow.None) return;
            
            var enemies = Resources.LoadAll("Enemies").Select(x => (GameObject) x).ToArray();

            for (int i = 0, n = enemies.Length; i < n; i++)
            {
                if(enemies[i].name == "Base") continue;
                
                _buttonFactory.Create(menu, content, enemies[i]);
            }
        }

        private void UpdateWindow(MenuWindowOpen data)
        {
            if (menu == data.Menu)
            {
                _cv.Show();
            }
            else
            {
                _cv.Hide();
            }
        }
    }
}