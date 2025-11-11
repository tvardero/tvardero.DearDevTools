using System;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace ta.UIKit.Nodes;

public class ColorRectangle : AreaNode
{
    private FSprite _sprite = null!;

    /// <inheritdoc />
    public ColorRectangle(ILogger<ColorRectangle>? logger = null) : base(logger) { }

    public Color Color { get; set; }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        _sprite.RemoveFromContainer(); // ensure removed
        _sprite = null!;
    }

    /// <param name="deltaTime"> </param>
    /// <inheritdoc />
    protected override void OnDraw(TimeSpan deltaTime)
    {
        base.OnDraw(deltaTime);

        _sprite.x = LocalPosition.x;
        _sprite.y = LocalPosition.y;
        _sprite.scaleX = Size.x;
        _sprite.scaleY = Size.y;
        _sprite.color = Color;
        _sprite.isVisible = IsVisible;
    }

    /// <inheritdoc />
    protected override void OnInitialize()
    {
        base.OnInitialize();

        _sprite = new FSprite("pixel");
        Futile.stage.AddChild(_sprite);
    }
}
