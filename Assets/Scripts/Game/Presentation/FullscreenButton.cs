using System;
using Graphene.Game.Systems.Input;
using Graphene.UiGenerics;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class FullscreenButton : ButtonView
    {
        [Inject] private InputSettings _inputSettings;
        private Text _tx;

        private void Setup()
        {
            _tx = transform.GetComponentInChildren<Text>();
        }

        private void Start()
        {
            SetText();
        }

        private void SetText()
        {
            _tx.text = Screen.fullScreen ? "Fullscreen" : "Exit Fullscreen";
        }

        protected override void OnClick()
        {
            base.OnClick();
            
            Screen.fullScreen = !Screen.fullScreen;
            SetText();
        }
    }
}