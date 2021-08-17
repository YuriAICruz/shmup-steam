using System.Collections;
using System.Collections.Generic;
using Graphene.LevelEditor;
using Graphene.LevelEditor.Presentation;
using Graphene.UiGenerics;
using UnityEngine;
using Zenject;

public class ToolButton : ButtonView
{
    public ToolType type;

    [Inject] private SignalBus _signalBus;
    [Inject] private EditorSettings _settings;

    void Setup()
    {
        _signalBus.Subscribe<ToolSelection>(UpdateButton);

        Button.image.color = _settings.normalColor;

        if (type == ToolType.Select)
            _signalBus.Fire(new ToolSelection(type));
    }

    private void UpdateButton(ToolSelection data)
    {
        Button.image.color = data.Type == type ? _settings.selectedColor : _settings.normalColor;
    }

    protected override void OnClick()
    {
        base.OnClick();

        _signalBus.Fire(new ToolSelection(type));
    }
}