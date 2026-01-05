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
    private readonly GameStateService _gameStateService;
    private bool _debugLogActive;
    private bool _debugMetricsActive;

    public MainMenuBar(MenuManager menuManager, IServiceProvider serviceProvider, ILogger<MainMenuBar> logger, GameStateService gameStateService)
        : base(logger: logger)
    {
        _menuManager = menuManager;
        _gameStateService = gameStateService;
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
            if (!_gameStateService.IsInGame) ImGui.BeginDisabled();

            if (ImGui.BeginMenu("Tools"))
            {
                MenuBarTools();
                ImGui.EndMenu();
            }

            if (!_gameStateService.IsInGame) ImGui.EndDisabled();

            if (ImGui.BeginMenu("Help"))
            {
                MenuBarHelp();
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        if (_debugLogActive) ImGui.ShowDebugLogWindow();
        if (_debugMetricsActive) ImGui.ShowMetricsWindow();
    }

    protected virtual void ProcessShortcuts()
    {
        if (ImGui.Shortcut(ImGuiKey.F1, ImGuiInputFlags.RouteGlobal)) _menuManager.EnsureShown<HelpMenu>();
    }

    private void MenuBarHelp()
    {
        if (ImGui.MenuItem("How to use Dear Dev Tools?", "F1")) _menuManager.EnsureShown<HelpMenu>();

        if (ImGui.MenuItem("Whats new?")) _menuManager.EnsureShown<WhatsNewMenu>();

        if (ImGui.MenuItem("Steam Workshop page")) Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=3417372413");

        if (ImGui.MenuItem("GitHub page")) Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools");

        if (ImGui.MenuItem("Report issue / suggest an idea")) Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools/issues");

        if (ImGui.MenuItem("Support development (ko-fi)")) Application.OpenURL("https://ko-fi.com/tvardero");

        ImGui.Separator();

        if (ImGui.MenuItem("ImGui debug logs", _debugLogActive)) _debugLogActive = !_debugLogActive;

        if (ImGui.MenuItem("ImGui debug metrics", _debugMetricsActive)) _debugMetricsActive = !_debugMetricsActive;

        ImGui.Separator();

        if (ImGui.MenuItem("Escape the end", "Esc + End")) _endEscaperService.EscapeTheEnd();
    }

    private void MenuBarTools()
    {
        if (ImGui.MenuItem("Palette editor")) _menuManager.CreateNew<PaletteEditorMenu>();
    }
}