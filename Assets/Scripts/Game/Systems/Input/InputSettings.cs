using System;
using UnityEngine;

namespace Graphene.Game.Systems.Input
{
    [Serializable]
    public class InputSettings
    {
        public enum InputState
        {
            Null = 0,
            Ui = 1,
            Game = 2
        }

        [Serializable]
        public class Input
        {
            public enum Action
            {
                Null = 0,
                MoveUp = 10,
                MoveDown = 11,
                Accept = 12,
                Cancel = 13,
                Shoot = 30,
                RapidFire = 31,
                Dodge = 32,
                Bomb = 33
            }

            public string description;
            public KeyCode[] key;
            public Action action;

            [Space] public bool useAxis;
            public string axis;
            public float cap = 0.1f;

            [Space] public bool constantCapture;
        }

        public bool Mouse => useMouse;

        public InputState State { get; private set; }

        public void SetState(InputState state)
        {
            State = state;
        }

        public void SetUseMouse(bool mouse)
        {
            useMouse = mouse;
        }
            
        [SerializeField]
        private bool useMouse;

        public Input[] UiInputs;
        public Input[] GameInputs;
    }
}