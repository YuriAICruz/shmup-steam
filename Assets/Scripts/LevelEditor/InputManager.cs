using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor
{
    public class InputEventUp
    {
        public InputManager.Input input;

        public InputEventUp(InputManager.Input input)
        {
            this.input = input;
        }
    }

    public class InputEventDown
    {
        public InputManager.Input input;

        public InputEventDown(InputManager.Input input)
        {
            this.input = input;
        }
    }

    public class InputEvent
    {
        public InputManager.Input input;

        public InputEvent(InputManager.Input input)
        {
            this.input = input;
        }
    }

    public class InputManager : ITickable
    {
        public enum Input
        {
            Null = 0,
            MouseLeft = 1,
            MouseRight = 2
        }

        private readonly InputEvent[] _mouse;
        private readonly InputEventUp[] _mouseUp;
        private readonly InputEventDown[] _mouseDown;

        private readonly SignalBus _signalBus;

        public InputManager(SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _mouse = new[]
            {
                new InputEvent(Input.MouseLeft),
                new InputEvent(Input.MouseRight),
            };
            _mouseUp = new[]
            {
                new InputEventUp(Input.MouseLeft),
                new InputEventUp(Input.MouseRight),
            };
            _mouseDown = new[]
            {
                new InputEventDown(Input.MouseLeft),
                new InputEventDown(Input.MouseRight),
            };
        }

        public void Tick()
        {
            GetInputs();
        }

        private void GetInputs()
        {

            for (int i = 0; i < 2; i++)
            {
                if (UnityEngine.Input.GetMouseButtonDown(i))
                {
                    MouseDown(i);
                }
                else if (UnityEngine.Input.GetMouseButtonUp(i))
                {
                    MouseUp(i);
                }
                else if (UnityEngine.Input.GetMouseButton(i))
                {
                    Mouse(i);
                }
            }
        }

        private void MouseDown(int i)
        {
            _signalBus.Fire(_mouseDown[i]);
        }

        private void MouseUp(int i)
        {
            _signalBus.Fire(_mouseUp[i]);
        }

        private void Mouse(int i)
        {
            _signalBus.Fire(_mouse[i]);
        }
    }
}