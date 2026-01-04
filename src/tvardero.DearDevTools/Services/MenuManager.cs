using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Services;

public class MenuManager
{
    private readonly ModImGuiContext _modImGuiContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public MenuManager(IServiceProvider serviceProvider, ILogger<MenuManager> logger)
    {
        _serviceProvider = serviceProvider;
        _modImGuiContext = serviceProvider.GetRequiredService<ModImGuiContext>();
        _logger = logger;
    }

    public IReadOnlyList<ImGuiDrawableBase> AllDrawables => _modImGuiContext.RenderList;

    public TDrawable CreateNew<TDrawable>(bool stealFocus = false)
    where TDrawable : ImGuiDrawableBase
    {
        _modImGuiContext.SanitizeRenderList();

        TDrawable? first = _modImGuiContext.RenderList.OfType<TDrawable>().FirstOrDefault();
        if (first is { AllowsMultipleInstances: false }) throw new InvalidOperationException("This drawable type does not allow multiple instances");

        return CreateNew_Impl<TDrawable>(stealFocus);
    }

    public void Destroy<TDrawable>(TDrawable drawable)
    where TDrawable : ImGuiDrawableBase
    {
        _modImGuiContext.RemoveDrawable(drawable);
        drawable.Dispose();
    }

    public void DestroyAllOfType<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        _logger.LogInformation("Destroying all drawables of type {DrawableType}", typeof(TDrawable));

        TDrawable[] toDestroy = _modImGuiContext.RenderList.OfType<TDrawable>().ToArray();
        foreach (TDrawable drawable in toDestroy) { Destroy(drawable); }
    }

    public TDrawable EnsureShown<TDrawable>(bool stealFocus = true)
    where TDrawable : ImGuiDrawableBase
    {
        var drawable = GetFirstOrCreateNew<TDrawable>();

        var window = drawable as ImGuiWindowBase;

        if (window is { IsOpen: false }) window.Reopen();
        drawable.Show();
        if (stealFocus) window?.Focus();

        return drawable;
    }

    public TDrawable GetFirstOrCreateNew<TDrawable>(bool stealFocus = false)
    where TDrawable : ImGuiDrawableBase
    {
        _modImGuiContext.SanitizeRenderList();

        TDrawable? drawable = _modImGuiContext.RenderList.OfType<TDrawable>().FirstOrDefault();
        if (drawable != null) return drawable;

        return CreateNew_Impl<TDrawable>(stealFocus);
    }

    public void HideAllOfType<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        _logger.LogInformation("Hiding all drawables of type {DrawableType}", typeof(TDrawable));

        _modImGuiContext.SanitizeRenderList();

        foreach (TDrawable drawable in _modImGuiContext.RenderList.OfType<TDrawable>()) { drawable.Hide(); }
    }

    public void ShowAllOfType<TDrawable>(bool show = true)
    where TDrawable : ImGuiDrawableBase
    {
        if (show) _logger.LogInformation("Showing all drawables of type {DrawableType}", typeof(TDrawable));
        else _logger.LogInformation("Hiding all drawables of type {DrawableType}", typeof(TDrawable));

        _modImGuiContext.SanitizeRenderList();

        foreach (TDrawable drawable in _modImGuiContext.RenderList.OfType<TDrawable>()) { drawable.Show(show); }
    }

    private TDrawable CreateNew_Impl<TDrawable>(bool stealFocus)
    where TDrawable : ImGuiDrawableBase
    {
        _logger.LogInformation("Creating new instance of type {DrawableType}", typeof(TDrawable));

        TDrawable drawable;
        try { drawable = ActivatorUtilities.GetServiceOrCreateInstance<TDrawable>(_serviceProvider); }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create instance of {DrawableType}, did you register all necessary service types?", typeof(TDrawable));
            throw;
        }

        _modImGuiContext.AddDrawable(drawable);

        if (stealFocus && drawable is ImGuiWindowBase window) window.Focus();

        return drawable;
    }
}