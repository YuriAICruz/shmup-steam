using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor.Presentation
{
    public class ToolSelection
    {
        public readonly ToolType Type;

        public ToolSelection(ToolType type)
        {
            Type = type;
        }
    }

    public enum ToolType
    {
        Select = 0,
        Move = 1,
        Rotate = 2,
        Scale = 3,
        Brush = 4,
        Eraser = 5
    }

    public class BrushReleased
    {
    }

    public class BrushSelected
    {
        public readonly MenuWindow Menu;
        public readonly GameObject Target;
        public readonly Sprite Icon;

        public BrushSelected(MenuWindow menu, GameObject target, Sprite icon)
        {
            Menu = menu;
            Target = target;
            Icon = icon;
        }
    }

    public class ToolsManager
    {
        private readonly SignalBus _signalBus;
        private ToolType _tool;
        private BrushSelected _brush;
        private LevelManager _levelManager;

        public bool HasBrush => _brush != null;
        public ToolType SelectedTool => _tool;

        public ToolsManager(SignalBus signalBus, LevelManager levelManager)
        {
            _signalBus = signalBus;
            _levelManager = levelManager;

            _signalBus.Subscribe<ToolSelection>(ToolSelected);
            _signalBus.Subscribe<BrushSelected>(BrushSelected);
        }

        private void BrushSelected(BrushSelected data)
        {
            _brush = data;

            _signalBus.Fire(new ToolSelection(ToolType.Brush));
        }

        private void ToolSelected(ToolSelection data)
        {
            _tool = data.Type;
            if (_tool != ToolType.Brush)
                ReleaseBrush();
        }

        public void ReleaseBrush()
        {
            if (!HasBrush) return;

            _brush = null;
            _signalBus.Fire<BrushReleased>();
        }

        public void CreateAsset(Vector3Int pos)
        {
            if (!HasBrush) return;

            _levelManager.CreateAsset(pos, _brush.Menu, _brush.Target);
        }
    }
}