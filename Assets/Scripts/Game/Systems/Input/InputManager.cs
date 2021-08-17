using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Input
{
    public class InputManager : ITickable
    {
        private readonly InputSettings _settings;
        private readonly SignalBus _signalBus;

        public class InputAxisEvent
        {
            public uint index;
            public Vector2 axis;

            public InputAxisEvent(uint index)
            {
                this.index = index;
            }
        }

        public class InputDownEvent
        {
            public InputDownEvent(InputSettings.Input.Action action)
            {
                this.action = action;
            }

            public readonly InputSettings.Input.Action action;
        }

        public class InputUpEvent
        {
            public InputUpEvent(InputSettings.Input.Action action)
            {
                this.action = action;
            }

            public readonly InputSettings.Input.Action action;
        }

        public class InputConstantEvent
        {
            public InputConstantEvent(InputSettings.Input.Action action)
            {
                this.action = action;
            }

            public readonly InputSettings.Input.Action action;
            public bool down;
        }

        public class InputEventHolder
        {
            public InputDownEvent down;
            public InputUpEvent up;
            public InputConstantEvent constant;
        }

        private InputEventHolder[] uiEvents;
        private InputEventHolder[] gameEvents;
        private InputAxisEvent _leftAxis;
        private InputAxisEvent _rightAxis;

        private Dictionary<string, bool> _axisToButton;

        public InputManager(InputSettings settings, SignalBus signalBus)
        {
            _settings = settings;
            _signalBus = signalBus;

            _axisToButton = new Dictionary<string, bool>();

            foreach (var input in _settings.UiInputs)
            {
                if (input.useAxis && !_axisToButton.ContainsKey(input.axis))
                    _axisToButton.Add(input.axis, false);
            }

            foreach (var input in _settings.GameInputs)
            {
                if (input.useAxis && !_axisToButton.ContainsKey(input.axis))
                    _axisToButton.Add(input.axis, false);
            }

            _leftAxis = new InputAxisEvent(0);
            _rightAxis = new InputAxisEvent(1);

            CreateHolders();
        }

        private void CreateHolders()
        {
            uiEvents = new InputEventHolder[_settings.UiInputs.Length];
            for (int i = 0; i < _settings.UiInputs.Length; i++)
            {
                uiEvents[i] = new InputEventHolder();
                uiEvents[i].down = new InputDownEvent(_settings.UiInputs[i].action);
                uiEvents[i].up = new InputUpEvent(_settings.UiInputs[i].action);
                uiEvents[i].constant = new InputConstantEvent(_settings.UiInputs[i].action);
            }

            gameEvents = new InputEventHolder[_settings.GameInputs.Length];
            for (int i = 0; i < _settings.GameInputs.Length; i++)
            {
                gameEvents[i] = new InputEventHolder();
                gameEvents[i].down = new InputDownEvent(_settings.GameInputs[i].action);
                gameEvents[i].up = new InputUpEvent(_settings.GameInputs[i].action);
                gameEvents[i].constant = new InputConstantEvent(_settings.GameInputs[i].action);
            }
        }

        public void Tick()
        {
            switch (_settings.State)
            {
                case InputSettings.InputState.Ui:
                    ReadUiInputs();
                    break;
                case InputSettings.InputState.Game:
                    ReadGameInputs();
                    break;
            }
        }

        private void ReadUiInputs()
        {
            for (int i = 0; i < _settings.UiInputs.Length; i++)
            {
                CheckInputCollection(_settings.UiInputs[i], uiEvents[i]);
            }
        }

        private void ReadGameInputs()
        {
            for (int i = 0; i < _settings.GameInputs.Length; i++)
            {
                CheckInputCollection(_settings.GameInputs[i], gameEvents[i]);
            }

            _leftAxis.axis.x = UnityEngine.Input.GetAxisRaw("Horizontal");
            _leftAxis.axis.y = UnityEngine.Input.GetAxisRaw("Vertical");
            _rightAxis.axis.x = UnityEngine.Input.GetAxisRaw("Horizontal2");
            _rightAxis.axis.y = UnityEngine.Input.GetAxisRaw("Vertical2");

            _signalBus.Fire(_leftAxis);
            _signalBus.Fire(_rightAxis);
        }

        private void CheckInputCollection(InputSettings.Input input, InputEventHolder events)
        {
            events.constant.down = false;

            UseAxis(input, events);

            for (int i = 0; i < input.key.Length; i++)
            {
                if (UnityEngine.Input.GetKeyDown(input.key[i]))
                {
                    _signalBus.Fire(events.down);
                }
                else if (UnityEngine.Input.GetKeyUp(input.key[i]))
                {
                    _signalBus.Fire(events.up);
                }

                if (input.constantCapture)
                {
                    events.constant.down |= UnityEngine.Input.GetKey(input.key[i]);
                    _signalBus.Fire(events.constant);
                }
            }
        }

        private void UseAxis(InputSettings.Input input, InputEventHolder events)
        {
            if (input.useAxis)
            {
                var value = UnityEngine.Input.GetAxis(input.axis);

                var down = false;
                if (input.cap >= 0)
                {
                    if(value < 0)
                        return;
                    down = value > input.cap;
                }
                else
                {
                    if(value > 0)
                        return;
                    down = value < input.cap;
                }

                if (down != _axisToButton[input.axis])
                {
                    if (down)
                        _signalBus.Fire(events.down);
                    else
                        _signalBus.Fire(events.up);

                    _axisToButton[input.axis] = down;
                }
            }
        }
    }
}