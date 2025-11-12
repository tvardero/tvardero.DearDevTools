using ta.UIKit.Inputs;
using ta.UIKit.Nodes;

namespace ta.UIKit;

public class SceneManager : IDisposable
{
    private readonly InputManager _inputManager;

    public SceneManager(InputManager inputManager)
    {
        _inputManager = inputManager;
    }

    public SceneRootNode? CurrentScene { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        CurrentScene?.Dispose();
    }

    public void ProcessDraw(TimeSpan elapsed)
    {
        CurrentScene?.ProcessDraw(elapsed);
    }

    public void ProcessInputEvents()
    {
        _inputManager.Collect();
        foreach (InputEvent inputEvent in _inputManager.GetNextEvents())
        {
            CurrentScene?.ProcessInputEvent(inputEvent);
            _inputManager.Return(inputEvent);
        }
    }

    public void ProcessUpdate(TimeSpan elapsed)
    {
        CurrentScene?.ProcessUpdate(elapsed);
    }

    public void SwitchScene(SceneRootNode? scene)
    {
        CurrentScene?.NotifySceneNoLongerCurrent();
        CurrentScene = scene;
        CurrentScene?.NotifySceneIsNowCurrent();
    }
}
