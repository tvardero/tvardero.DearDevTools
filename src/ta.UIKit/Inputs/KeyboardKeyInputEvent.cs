using UnityEngine;

namespace ta.UIKit.Inputs;

public record KeyboardKeyInputEvent : InputEvent
{
    public KeyCode KeyCode { get; private set; } = KeyCode.None;

    public bool IsDown { get; private set; }

    public bool IsUp => !IsDown;

    public bool IsEcho { get; private set; }

    public bool IsJustPressed => IsDown && !IsEcho;

    public bool IsJustReleased => IsUp; // we never echo up events

    /// <inheritdoc />
    public override bool TryReset()
    {
        KeyCode = KeyCode.None;
        IsDown = false;
        IsEcho = false;

        return base.TryReset();
    }

    public void Set(KeyCode keyCode, bool isDown, bool isEcho)
    {
        KeyCode = keyCode;
        IsDown = isDown;
        IsEcho = isEcho;
    }
}
