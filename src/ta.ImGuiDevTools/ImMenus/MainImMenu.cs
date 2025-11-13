using System.Security;
using ImGuiNET;
using JetBrains.Annotations;

namespace ta.ImGuiDevTools.ImMenus;

[SuppressUnmanagedCodeSecurity]
public class MainImMenu
{
    [UsedImplicitly]
    public static void MenuCallback(ref nint idxgiSwapChain, ref uint syncInterval, ref uint flags)
    {
        ImGui.Text("ImGui Dev Tools says hello!");
    }
}
