using ImGuiNET;
using Silk.NET.Windowing;

namespace ta.ModMaker.Windows;

public class DemoWindow : ImGuiWindowBase
{
    /// <inheritdoc />
    private DemoWindow(WindowOptions windowOptions) : base(windowOptions) { }

    public static DemoWindow Create()
    {
        var windowOptions = WindowOptions.Default;

        windowOptions.Title = "ImGui Demo Window";

        return new DemoWindow(windowOptions);
    }

    protected override void OnImGuiRender(double frameTime)
    {
        ImGui.ShowDemoWindow();
    }
}
