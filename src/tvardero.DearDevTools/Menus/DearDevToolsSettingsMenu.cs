using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Menus;

public class DearDevToolsSettingsMenu : SimpleWindowBase
{
    /// <inheritdoc />
    public override string Name => "Dear Dev Tools Settings";

    /// <inheritdoc />
    protected override void OnDrawWindowContent() { }

    /// <inheritdoc />
    public override bool IsBlockingRWInput => true;
}
