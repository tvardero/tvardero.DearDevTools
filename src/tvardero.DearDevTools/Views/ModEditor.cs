using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Models;

namespace tvardero.DearDevTools.Views;

// TODO
public class ModEditor : ImGuiWindowWithLeftPanelBase
{
    private readonly SortedSet<ModModel> _coreMods = new(Comparer<ModModel>.Create((x, y) => string.CompareOrdinal(x.Id, y.Id)));
    private readonly SortedSet<ModModel> _userMods = new(Comparer<ModModel>.Create((x, y) => string.CompareOrdinal(x.Id, y.Id)));
    private readonly SortedSet<ModModel> _steamWorkshopMods = new(Comparer<ModModel>.Create((x, y) => string.CompareOrdinal(x.Id, y.Id)));
    private string? _selectedModId;

    /// <inheritdoc />
    public ModEditor() : base("Mod Editor")
    {
        ReloadList();
    }

    public void ReloadList()
    {
        // list core mods (hardcoded list)

        // list user mods (from mods/ directory, excluding core mods)

        // list steam workshop mods
    }

    /// <inheritdoc />
    protected override void OnDrawLeftPanel()
    {
        ImGui.Text("Core mods");

        // show core mods (watcher, msc, devtools, remix, etc.)
        // - do not allow modifying modinfo for any core mod
        // - show button to open files directory of the mod
        ListMods(_coreMods);

        ImGui.Separator();
        ImGui.Text("User mods");

        // show user mods from local mods/ directory
        // - allow modifying modinfo for local user mods
        // - show button to open steam workshop management
        // - show button to open files directory of the mod
        // - add options to create a file watcher for source directory of the mod
        ListMods(_userMods);

        ImGui.Separator();
        ImGui.Text("Steam Workshop mods");

        // show mods from steam workshop
        // - do not allow modifying modinfo for any steam workshop mod
        // - show button to open workshop page
        // - show button to open files directory of the mod (so if user wants - they can modify mod manually)
        ListMods(_steamWorkshopMods);

        return;

        void ListMods(IEnumerable<ModModel> mods)
        {
            foreach (ModModel? mod in mods) { ImGui.Selectable(mod.Name + "##" + mod.Id, mod.Id == _selectedModId); }
        }
    }

    /// <inheritdoc />
    protected override void OnDrawMiddleContent()
    {
        ImGui.BeginGroup();

        ImGui.EndGroup();
    }
}