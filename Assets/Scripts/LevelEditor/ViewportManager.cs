using System;
using System.Collections.Generic;
using Graphene.LevelEditor.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Graphene.LevelEditor
{
    public class CloseAssetMenu
    {
        
    }
    public class OpenAssetMenu
    {
        public readonly IClickable Target;

        public OpenAssetMenu(IClickable target)
        {
            Target = target;
        }
    }
    
    public class Viewport
    {
        private readonly SignalBus _signalBus;
        private readonly Grid _grid;
        private readonly float _panDelay;
        private readonly EditorCamera _camera;
        private readonly ToolsManager _toolsManager;
        private readonly LevelManager _levelManager;
        private readonly EventSystem _eventSystem;
        private readonly GraphicRaycaster _raycaster;

        enum State
        {
            Idle = 0,
            Panning = 1,
            Dragging = 2,
            Painting = 3
        }

        private State _state;
        private float _panTime;
        private Vector3 _lastMouse;
        private IClickable _target;

        public Viewport(SignalBus signalBus, Grid grid, float panDelay, EditorCamera camera, ToolsManager toolsManager,
            LevelManager levelManager, EventSystem eventSystem, GraphicRaycaster raycaster)
        {
            _signalBus = signalBus;
            _grid = grid;
            _panDelay = panDelay;
            _camera = camera;
            _toolsManager = toolsManager;
            _levelManager = levelManager;
            _eventSystem = eventSystem;
            _raycaster = raycaster;

            _signalBus.Subscribe<InputEventDown>(InputDown);
            _signalBus.Subscribe<InputEventUp>(InputUp);
            _signalBus.Subscribe<InputEvent>(InputEvent);
        }

        
        private Vector3Int GridPosition()
        {
            var mouseWorld = MouseWorld();

            var pos = _grid.WorldToCell(new Vector3(
                Mathf.Ceil(mouseWorld.x),
                Mathf.Ceil(mouseWorld.y),
                Mathf.Ceil(mouseWorld.z)
            ));

            return pos;
        }

        private Vector3 MouseWorld()
        {
            var mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition + CameraZ());

            return mouseWorld;
        }
        
        private Vector3 CameraZ()
        {
            var z = Mathf.Abs(_camera.transform.position.z) * Vector3.forward;
            return z;
        }
        

        private void InputUp(InputEventUp data)
        {
            switch (data.input)
            {
                case InputManager.Input.MouseLeft:
                    switch (_state)
                    {
                        case State.Idle:
                            InteractSingle();
                            break;
                        case State.Dragging:
                            _target?.Release(GridPosition());
                            _state = State.Idle;
                            _target = null;
                            break;
                        case State.Panning:
                        case State.Painting:
                            _state = State.Idle;
                            break;
                    }

                    break;
                case InputManager.Input.MouseRight:
                    if (_state == State.Idle && _toolsManager.HasBrush)
                        _toolsManager.ReleaseBrush();
                    break;
            }
        }

        private void InputDown(InputEventDown data)
        {
            switch (data.input)
            {
                case InputManager.Input.MouseLeft:
                    _panTime = 0;

                    if (_state == State.Idle)
                    {
                        Click();
                    }

                    break;
                case InputManager.Input.MouseRight:
                    switch (_state)
                    {
                        case State.Idle:
                            TryOpenTargetMenu();                            
                            break;
                    }
                    break;
            }
        }

        private void InputEvent(InputEvent data)
        {
            switch (data.input)
            {
                case InputManager.Input.MouseLeft:
                    switch (_state)
                    {
                        case State.Idle:
                            InteractContinuous();
                            break;
                        case State.Painting:
                            PaintContinuous();
                            break;
                        case State.Panning:
                            PanState();
                            break;
                        case State.Dragging:
                            DragState(MouseWorld());
                            break;
                    }

                    _lastMouse = Input.mousePosition;
                    break;
                case InputManager.Input.MouseRight:
                    break;
            }
        }


        private void PickTarget()
        {
            _target?.Release();

            if (!Raycast(Input.mousePosition, out _target))
                return;

            if (_target != null)
            {
                _target.Pick(GridPosition());
                _state = State.Dragging;
            }
        }


        private void PaintSingle()
        {
            if (!_toolsManager.HasBrush) return;

            _toolsManager.CreateAsset(GridPosition());
        }

        private void PaintContinuous()
        {
            if (!_toolsManager.HasBrush) return;

            _state = State.Painting;

            if (!_levelManager.IsEmpty(GridPosition()))
                return;

            if (!Raycast(Input.mousePosition, out _target))
                return;

            if (_target != null) return;

            PaintSingle();
        }

        private void Click()
        {
            switch (_toolsManager.SelectedTool)
            {
                case ToolType.Select:
                case ToolType.Move:
                case ToolType.Rotate:
                case ToolType.Scale:
                    PickTarget();
                    break;
                case ToolType.Brush:
                    break;
                case ToolType.Eraser:
                    EraseSingle();
                    break;
            }
        }

        private void InteractSingle()
        {
            switch (_toolsManager.SelectedTool)
            {
                case ToolType.Select:
                    break;
                case ToolType.Move:
                    break;
                case ToolType.Rotate:
                    break;
                case ToolType.Scale:
                    break;
                case ToolType.Brush:
                    PaintSingle();
                    break;
                case ToolType.Eraser:
                    EraseSingle();
                    break;
            }
        }

        private void InteractContinuous()
        {
            switch (_toolsManager.SelectedTool)
            {
                case ToolType.Select:
                case ToolType.Move:
                case ToolType.Rotate:
                case ToolType.Scale:
                    PanState();
                    break;
                case ToolType.Brush:
                    PaintContinuous();
                    break;
                case ToolType.Eraser:
                    EraseContinuous();
                    break;
            }
        }


        private void EraseSingle()
        {
            if (!Raycast(Input.mousePosition, out _target))
                return;

            if (_target == null || !_target.CanDelete) return;

            _levelManager.DestroyAsset(_target.GameObject);
        }

        private void EraseContinuous()
        {
            EraseSingle();
        }


        private void DragState(Vector3 pos)
        {
            _target?.Drag(pos);
        }

        private void PanState()
        {
            _panTime += Time.deltaTime;

            if (_panTime > _panDelay)
                Pan(Input.mousePosition, _lastMouse);
        }


        private void TryOpenTargetMenu()
        {
            _target?.Release();

            if (!Raycast(Input.mousePosition, out _target))
                return;
            
            if(_target == null || !_target.HasMenu()) return;
            
            _signalBus.Fire(new OpenAssetMenu(_target));
        }

        private bool Raycast(Vector3 position, out IClickable target)
        {
            var ray = _camera.ScreenPointToRay(position);
            target = null;

            var e = new PointerEventData(_eventSystem);
            e.position = Input.mousePosition;
            var res = new List<RaycastResult>();
            _raycaster.Raycast(e, res);

            if (res.Count > 0)
                return false;

            if (Physics.Raycast(ray, out var hit, _camera.farClipPlane))
            {
                var clickable = hit.transform.GetComponent<IClickable>();

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 5);
                target = clickable;
                return true;
            }

            Debug.DrawRay(ray.origin, ray.direction * _camera.farClipPlane, Color.magenta, 5);
            return true;
        }

        private void Pan(Vector3 position, Vector3 lastPosition)
        {
            var z = CameraZ();
            _state = State.Panning;
            var pos = _camera.ScreenToWorldPoint(position + z);
            var last = _camera.ScreenToWorldPoint(lastPosition + z);

            _camera.Pan(last - pos);
        }

    }
}