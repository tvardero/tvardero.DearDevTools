using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using UnityEngine;

namespace ta.UIKit.Inputs;

public class InputEventPool
{
    private readonly Dictionary<Type, object> _pools = new();

    public KeyboardKeyInputEvent GetKeyboardKeyInputEvent(KeyCode keyCode, bool isDown, bool isEcho)
    {
        var inputEvent = Get<KeyboardKeyInputEvent>();
        inputEvent.Set(keyCode, isDown, isEcho);
        return inputEvent;
    }

    public MouseButtonPressInputEvent GetMouseButtonPressInputEvent(KeyCode keyCode, bool isDown, bool isEcho)
    {
        var inputEvent = Get<MouseButtonPressInputEvent>();
        inputEvent.Set(keyCode, isDown, isEcho);
        return inputEvent;
    }

    public MouseMotionInputEvent GetMouseMotionInputEvent(Vector2 relativePosition, Vector2 currentPosition)
    {
        var inputEvent = Get<MouseMotionInputEvent>();
        inputEvent.Set(relativePosition, currentPosition);
        return inputEvent;
    }

    public JoystickButtonPressInputEvent GetJoystickButtonPressInputEvent(KeyCode keyCode, bool isDown, bool isEcho)
    {
        var inputEvent = Get<JoystickButtonPressInputEvent>();
        inputEvent.Set(keyCode, isDown, isEcho);
        return inputEvent;
    }

    public void Return<TInputEvent>(TInputEvent inputEvent)
    where TInputEvent : InputEvent
    {
        Type type = typeof(TInputEvent);
        if (!_pools.TryGetValue(type, out object? poolObj)) return;

        var pool = (ObjectPool<TInputEvent>)poolObj;
        pool.Return(inputEvent);
    }

    private TInputEvent Get<TInputEvent>()
    where TInputEvent : InputEvent, new()
    {
        Type type = typeof(TInputEvent);
        var pool = (ObjectPool<TInputEvent>)_pools.GetOrAdd(type, ObjectPool.Create<TInputEvent>());
        return pool.Get();
    }
}
