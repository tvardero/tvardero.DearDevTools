using UnityEngine;

namespace ta.UIKit.Inputs;

public record MouseMotionInputEvent : InputEvent
{
    public Vector2 RelativePosition { get; private set; } = Vector2.zero;

    public Vector2 CurrentPosition { get; private set; } = -Vector2.one;

    public void Set(Vector2 relativePosition, Vector2 currentPosition)
    {
        RelativePosition = relativePosition;
        CurrentPosition = currentPosition;
    }

    /// <inheritdoc />
    public override bool TryReset()
    {
        RelativePosition = Vector2.zero;
        CurrentPosition = -Vector2.one;

        return base.TryReset();
    }
}
