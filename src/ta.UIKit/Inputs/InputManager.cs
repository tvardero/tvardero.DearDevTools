using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using UnityEngine;

namespace ta.UIKit.Inputs;

public class InputManager
{
    private static readonly KeyCode[] _keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
    private readonly InputEventPool _inputEventPool;
    private readonly Queue<InputEvent> _inputEventsQueue = new(20);
    private readonly InputManagerOptions _options;
    private readonly Dictionary<KeyCode, int> _downKeysFrames = new();

    public InputManager(InputEventPool inputEventPool, IOptions<InputManagerOptions> options)
    {
        _inputEventPool = inputEventPool;
        _options = options.Value;
    }

    public Vector2 MousePosition { get; private set; } = Vector2.zero;

    public void EnqueueInputEvent(InputEvent inputEvent)
    {
        _inputEventsQueue.Enqueue(inputEvent);
    }

    public bool IsDown(KeyCode keyCode)
    {
        return _downKeysFrames.ContainsKey(keyCode) && _downKeysFrames[keyCode] > 1;
    }

    public bool IsJustDown(KeyCode keyCode)
    {
        return _downKeysFrames.ContainsKey(keyCode) && _downKeysFrames[keyCode] == 1;
    }

    public bool IsUp(KeyCode keyCode)
    {
        return _downKeysFrames.ContainsKey(keyCode);
    }

    internal void Collect()
    {
        CollectKeyCodeEvents();
        CollectMouseMotionEvent();
    }

    internal IEnumerable<InputEvent> GetNextEvents()
    {
        InputEvent[] queueCopy = _inputEventsQueue.ToArray();
        _inputEventsQueue.Clear();

        foreach (InputEvent inputEvent in queueCopy)
        {
            if (IsHandledByGame(inputEvent)) inputEvent.SetHandled();
            yield return inputEvent;
        }
    }

    internal void Return(InputEvent inputEvent)
    {
        _inputEventPool.Return(inputEvent);
    }

    private void CollectKeyCodeEvents()
    {
        // "up" events
        IEnumerable<KeyCode> upKeys = _downKeysFrames.Keys.Where(Input.GetKeyUp);
        foreach (KeyCode keyCode in upKeys)
        {
            _downKeysFrames.Remove(keyCode);
            InputEvent? inputEvent = CreateForKeyCode(keyCode, false, false);
            if (inputEvent != null) _inputEventsQueue.Enqueue(inputEvent);
        }

        // "down" events
        IEnumerable<KeyCode> downKeys = _keyCodes.Where(Input.GetKey);
        foreach (KeyCode keyCode in downKeys)
        {
            int framesDown = _downKeysFrames.TryGetValue(keyCode, out int framesDownPrev) ? framesDownPrev + 1 : 1;
            if (framesDown < _options.FramesBeforeEcho) _downKeysFrames[keyCode] = framesDown;

            if (framesDown == 1)
            {
                InputEvent? inputEvent = CreateForKeyCode(keyCode, true, false);
                if (inputEvent != null) _inputEventsQueue.Enqueue(inputEvent);
            }
            else if (framesDown >= _options.FramesBeforeEcho && ShouldProduceEchoForKeyCode(keyCode))
            {
                InputEvent? inputEvent = CreateForKeyCode(keyCode, true, true);
                if (inputEvent != null) _inputEventsQueue.Enqueue(inputEvent);
            }
        }
    }

    private bool ShouldProduceEchoForKeyCode(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.None => false,
            <= KeyCode.Menu => _options.ShouldProduceEchoForKeyboard,
            <= KeyCode.Mouse6 => _options.ShouldProduceEchoForMouseButton,
            <= KeyCode.Joystick8Button19 => _options.ShouldProduceEchoForJoystickButton,
            _ => false,
        };
    }

    private void CollectMouseMotionEvent()
    {
        Vector3 mousePosition3D = Input.mousePosition;
        Vector2 mousePosition = new(mousePosition3D.x, mousePosition3D.y);
        if (mousePosition == MousePosition) return;

        Vector2 mouseDelta = mousePosition - MousePosition;
        MousePosition = mousePosition;
        MouseMotionInputEvent inputEvent = _inputEventPool.GetMouseMotionInputEvent(mouseDelta, mousePosition);
        _inputEventsQueue.Enqueue(inputEvent);
    }

    private InputEvent? CreateForKeyCode(KeyCode keyCode, bool isDown, bool isEcho)
    {
        return keyCode switch
        {
            KeyCode.None => null,
            <= KeyCode.Menu => _inputEventPool.GetKeyboardKeyInputEvent(keyCode, isDown, isEcho),
            <= KeyCode.Mouse6 => _inputEventPool.GetMouseButtonPressInputEvent(keyCode, isDown, isEcho),
            <= KeyCode.Joystick8Button19 => _inputEventPool.GetJoystickButtonPressInputEvent(keyCode, isDown, isEcho),
            _ => null,
        };
    }

    private static bool IsHandledByGame(InputEvent inputEvent)
    {
        return false; // TODO
    }
}
