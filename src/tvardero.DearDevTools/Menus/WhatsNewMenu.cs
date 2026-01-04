using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Menus;

public class WhatsNewMenu : ImGuiWindowBase
{
    /// <inheritdoc />
    public WhatsNewMenu(ILogger<WhatsNewMenu> logger) : base("Whats new?", logger: logger) { }

    /// <inheritdoc />
    protected override void OnDrawWindowContent() { }
}