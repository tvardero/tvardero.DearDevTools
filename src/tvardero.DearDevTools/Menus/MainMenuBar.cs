using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Services;
using UnityEngine;

namespace tvardero.DearDevTools.Menus;

public class MainMenuBar : ImGuiDrawableBase
{
    private readonly MenuManager _menuManager;
    private readonly EndEscaperService _endEscaperService;
    private readonly DearDevToolsPlugin _plugin;
    private bool _debugLogActive;
    private bool _debugMetricsActive;
    private bool _demoWindowActive;

    public MainMenuBar(
        MenuManager menuManager,
        IServiceProvider serviceProvider,
        ILogger<MainMenuBar> logger,
        DearDevToolsPlugin plugin)
        : base(logger: logger)
    {
        _menuManager = menuManager;
        _plugin = plugin;
        _endEscaperService = serviceProvider.GetRequiredService<EndEscaperService>();
    }

    /// <inheritdoc />
    public override bool IsVisible => true;

    /// <inheritdoc />
    public override bool IsBlockingWMEvent => false;

    /// <inheritdoc />
    public override bool RequiresMainUiVisible => true;

    /// <inheritdoc />
    protected internal override void Draw()
    {
        ProcessShortcuts();

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Menu"))
            {
                MenuBarMenu();
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                MenuBarHelp();
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        if (_demoWindowActive) ImGui.ShowDemoWindow();
        if (_debugLogActive) ImGui.ShowDebugLogWindow();
        if (_debugMetricsActive) ImGui.ShowMetricsWindow();
    }

    protected virtual void ProcessShortcuts()
    {
        if (ImGui.Shortcut(ImGuiKey.F1, ImGuiInputFlags.RouteGlobal)) { } // TODO
    }

    private void MenuBarMenu()
    {
        if (ImGui.MenuItem("Bad menu")) { _menuManager.CreateNew<BadMenu>(); }

        ImGui.Separator();

        if (ImGui.MenuItem("Hide Dear Dev Tools UI", "Ctrl + H")) { _plugin.IsMainUiVisible = false; }

        if (ImGui.MenuItem("Deactivate Dear Dev Tools", "Ctrl + O")) { _plugin.IsActivated = false; }
    }

    private void MenuBarHelp()
    {
        if (ImGui.MenuItem("How to use Dear Dev Tools?", "F1")) { } // TODO

        if (ImGui.MenuItem("Whats new?")) { } // TODO

        if (ImGui.MenuItem("Steam Workshop page")) Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=3417372413");

        if (ImGui.MenuItem("GitHub page")) Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools");

        if (ImGui.MenuItem("Report issue / suggest an idea")) Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools/issues");

        if (ImGui.MenuItem("Support development (ko-fi)")) Application.OpenURL("https://ko-fi.com/tvardero");

        ImGui.Separator();

        ImGui.MenuItem("ImGui demo window", null, ref _demoWindowActive);
        ImGui.MenuItem("ImGui debug logs", null, ref _debugLogActive);
        ImGui.MenuItem("ImGui debug metrics", null, ref _debugMetricsActive);

        ImGui.Separator();

        if (ImGui.MenuItem("Escape the end", "Esc + End")) _endEscaperService.EscapeTheEnd();
    }
}