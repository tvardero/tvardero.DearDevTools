using ImGuiNET;

namespace tvardero.MenuTests;

public class TestSubject
{
    private const string _DEFAULT_TITLE = "Palette editor###Palette editor";

    private bool _isOpen = true;
    private string _windowTitle = _DEFAULT_TITLE;

    // room info
    private bool _roomLoaded;
    private string? _roomName;
    private bool _roomHasTemplate;
    private int _roomScreens = 1;

    // palette selections
    private int _palette;
    private bool _useTemplatePalette;
    private int _effectA;
    private bool _useTemplateEffectA;
    private int _effectB;
    private bool _useTemplateEffectB;
    private bool _paletteSelectorOpen;

    // all palettes info
    private List<int> _availablePalettes = [];
    private bool _fileWatchedEnabled;

    public TestSubject()
    {
        Load();
    }

    public bool IsOpen => _isOpen;

    public void Draw()
    {
        ImGui.Begin(_windowTitle, ref _isOpen, ImGuiWindowFlags.MenuBar);

        ImGui.BeginMenuBar();

        if (ImGui.BeginMenu("Palettes"))
        {
            ImGui.MenuItem("Create");
            ImGui.MenuItem("Edit");
            if (ImGui.MenuItem("Watch file changes", _fileWatchedEnabled))
            {
                // Enable file watched
            }

            ImGui.EndMenu();
        }

        ImGui.MenuItem("Force re-apply");

        ImGui.EndMenuBar();

        // Palette selection
        ImGui.Checkbox("<T>##Palette", ref _useTemplatePalette);

        ImGui.SameLine();
        ImGui.Text("Palette: " + _palette);

        ImGui.PushItemFlag(ImGuiItemFlags.ButtonRepeat, true);

        ImGui.SameLine();
        if (ImGui.ArrowButton("##PreviousPalette", ImGuiDir.Left)) _palette--;

        ImGui.SameLine();
        if (ImGui.Button("Select##SelectPalette")) _paletteSelectorOpen = true;

        ImGui.SameLine();
        if (ImGui.ArrowButton("##NextPalette", ImGuiDir.Right)) _palette++;

        ImGui.PopItemFlag();

        // Effect A selection

        ImGui.End();
    }

    public void Load()
    {
        _roomName = "SS_AI";
        _roomHasTemplate = true;
        _windowTitle = $"Palette editor - {_roomName}###Palette editor";
        _roomLoaded = true;
    }

    public void Reset()
    {
        _roomName = null;
        _roomHasTemplate = false;
        _palette = 0;
        _useTemplatePalette = false;
        _effectA = 0;
        _useTemplateEffectA = false;
        _effectB = 0;
        _useTemplateEffectB = false;
        _windowTitle = _DEFAULT_TITLE;
        _roomLoaded = false;
    }
}