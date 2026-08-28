using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Menus;

public class BadMenu : ImGuiWindowBase
{
    private readonly DateTime _crashAt;

    /// <inheritdoc />
    public BadMenu(ILogger<BadMenu>? logger = null) : base("Bad menu", logger: logger)
    {
        _crashAt = DateTime.Now + TimeSpan.FromSeconds(5);
    }

    /// <inheritdoc />
    protected override void OnDrawWindowContent()
    {
        if (DateTime.Now >= _crashAt) ImGui.EndDisabled();
    }
}