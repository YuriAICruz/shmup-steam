using Graphene.Game.Systems.Input;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Presentation
{
    public class InputDebugger
    {
        private readonly SignalBus _signalBus;

        public InputDebugger(SignalBus signalBus)
        {
            _signalBus = signalBus;

            _signalBus.Subscribe<InputManager.InputDownEvent>(OnDown);
            _signalBus.Subscribe<InputManager.InputUpEvent>(OnUp);
            _signalBus.Subscribe<InputManager.InputConstantEvent>(InputUpdate);
            _signalBus.Subscribe<InputManager.InputAxisEvent>(InputAxisUpdate);
        }

        private void InputAxisUpdate(InputManager.InputAxisEvent input)
        {
            //Debug.Log(input.index + ":" + input.axis);
        }

        private void InputUpdate(InputManager.InputConstantEvent input)
        {
            if (input.down)
                Debug.Log(input.action);
        }

        private void OnUp(InputManager.InputUpEvent input)
        {
            Debug.Log("Up: " + input.action);
        }

        private void OnDown(InputManager.InputDownEvent input)
        {
            Debug.Log("Down: " + input.action);
        }
    }
}