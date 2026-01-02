using System.Drawing;
using System.Globalization;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using tvardero.MenuTests;

WindowOptions windowOptions = WindowOptions.Default with
{
    Title = "Dear Dev Tools tests",
};

IWindow window = Window.Create(windowOptions);

GL gl = null!;
IInputContext input = null!;
ImGuiController imguiCtl = null!;
ImGuiIOPtr io = 0;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("uk");

var testSubject = new TestSubject();

window.Load += OnLoad;
window.FramebufferResize += size => gl.Viewport(size);
window.Update += OnUpdate;
window.Render += OnRender;
window.Closing += OnClosing;

window.Run();
window.Dispose();

return 0;

void OnLoad()
{
    gl = window.CreateOpenGL();

    input = window.CreateInput();

    imguiCtl = new ImGuiController(gl,
        window,
        input,
        () =>
        {
            unsafe
            {
                io = ImGui.GetIO();

                ImFontGlyphRangesBuilder* glyphRangeBuilder = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
                var glyphRangeBuilderPtr = new ImFontGlyphRangesBuilderPtr(glyphRangeBuilder);

                glyphRangeBuilderPtr.AddRanges(io.Fonts.GetGlyphRangesCyrillic());

                glyphRangeBuilderPtr.BuildRanges(out ImVector ranges);
                glyphRangeBuilderPtr.Destroy();

                io.Fonts.AddFontFromFileTTF("FiraCode-Light.ttf", 18f, null, ranges.Data);
                io.Fonts.Build();
            }
        });

    imguiCtl.MakeCurrent();
}

void OnRender(double delta)
{
    imguiCtl.Update((float)delta);

    gl.ClearColor(Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
    gl.Clear((uint)ClearBufferMask.ColorBufferBit);

    OnRenderImgui();

    imguiCtl.Render();
}

void OnUpdate(double delta)
{
    IKeyboard keyboard = input.Keyboards[0];

    io.AddKeyEvent(ImGuiKey.ModAlt, keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight));
    io.AddKeyEvent(ImGuiKey.ModSuper, keyboard.IsKeyPressed(Key.SuperLeft) || keyboard.IsKeyPressed(Key.SuperRight));
    io.AddKeyEvent(ImGuiKey.ModCtrl, keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight));
    io.AddKeyEvent(ImGuiKey.ModShift, keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight));
}

void OnClosing()
{
    imguiCtl.Dispose();
    input.Dispose();
    gl.Dispose();
}

void OnRenderImgui()
{
    ImGui.ShowDemoWindow();

    if (!testSubject.IsOpen) window.Close();
    else testSubject.Draw();
}