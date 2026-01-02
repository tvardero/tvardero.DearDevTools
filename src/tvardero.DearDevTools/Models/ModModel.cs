namespace tvardero.DearDevTools.Models;

public record ModModel(string Id, string Name, bool IsCore, bool IsUser, bool IsSteam, bool IsEnabled, string ModAbsolutePath)
{
    public bool CanEditModInfo => IsUser;
}