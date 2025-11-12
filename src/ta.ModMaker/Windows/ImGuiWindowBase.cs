using System.Drawing;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ta.ModMaker.Windows;

public abstract class ImGuiWindowBase : IDisposable
{
    private readonly ImGuiController _imGuiController;

    protected ImGuiWindowBase(WindowOptions windowOptions)
    {
        Window = Silk.NET.Windowing.Window.Create(windowOptions);
        Window.Initialize();

        GL = Window.CreateOpenGL();
        Input = Window.CreateInput();
        _imGuiController = new ImGuiController(GL, Window, Input);

        ImGui.GetIO().MouseDrawCursor = true;
        Input.Mice[0].Cursor.CursorMode = CursorMode.Hidden;

        Window.FramebufferResize += s => GL.Viewport(s);
        Window.Update += OnUpdate;
        Window.Render += OnRender;
        Window.Closing += OnClosing;
    }

    public Color BackgroundColor { get; set; } = Color.DarkGray;

    protected IWindow Window { get; }

    protected GL GL { get; }

    protected IInputContext Input { get; }

    public void Dispose()
    {
        OnDispose();

        _imGuiController.Dispose();
        Input.Dispose();
        GL.Dispose();

        GC.SuppressFinalize(this);
    }

    public void Run()
    {
        Window.Run();
        Window.Dispose();
    }

    protected virtual void OnClosing()
    {
        Dispose();
    }

    protected void OnDispose() { }

    protected abstract void OnImGuiRender(double frameTime);

    protected virtual void OnRender(double frameTime)
    {
        _imGuiController.Update((float)frameTime);

        GL.ClearColor(BackgroundColor);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        OnImGuiRender(frameTime);

        _imGuiController.Render();
    }

    protected virtual void OnUpdate(double frameTime) { }
}
