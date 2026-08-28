using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Menus;

namespace tvardero.DearDevTools.Services;

public class MenuManager
{
    private static readonly Type[] _criticalDrawables = [typeof(MainMenuBar), typeof(DearDevToolsEnabledOverlay)];
    private readonly ModImGuiContext _modImGuiContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MenuManager> _logger;

    public MenuManager(IServiceProvider serviceProvider, ILogger<MenuManager> logger)
    {
        _modImGuiContext = serviceProvider.GetRequiredService<ModImGuiContext>();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IReadOnlyList<ImGuiDrawableBase> AllDrawables => _modImGuiContext.RenderList;

    public TDrawable CreateNew<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        _logger.LogDebug("Creating new instance of type {DrawableType}", typeof(TDrawable));

        var drawable = CreateNew_Impl<TDrawable>();
        if (AllDrawables.Contains(drawable))
        {
            _logger.LogDebug("The same instance is already present in drawables list");
            throw new InvalidOperationException("Singleton instance cannot be created more than once.");
        }

        _modImGuiContext.AddDrawable(drawable);
        return drawable;
    }

    public void Destroy(ImGuiDrawableBase drawable)
    {
        if (_criticalDrawables.Contains(drawable.GetType())) throw new InvalidOperationException("Destroying this drawable is not allowed.");

        _logger.LogDebug("Destroying drawable {Drawable}", drawable);

        _modImGuiContext.RemoveDrawable(drawable);
        drawable.Dispose();
    }

    public void DestroyAllOfType<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable[] toDestroy = AllDrawables
            .Where(d => !d.IsDisposed)
            .OfType<TDrawable>()
            .Where(d => !_criticalDrawables.Contains(d.GetType()))
            .ToArray();

        foreach (TDrawable drawable in toDestroy) { Destroy(drawable); }
    }

    public TDrawable? GetFirstOfType<TDrawable>(IComparer<TDrawable>? comparer = null)
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable? first = comparer == null
            ? AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>().FirstOrDefault()
            : AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>().OrderBy(d => d, comparer).FirstOrDefault();

        return first;
    }

    public TDrawable GetFirstOrCreateNew<TDrawable>(IComparer<TDrawable>? comparer = null)
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable? drawable = GetFirstOfType(comparer);
        return drawable ?? CreateNew<TDrawable>();
    }

    public TDrawable? GetLastOfType<TDrawable>(IComparer<TDrawable>? comparer = null)
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable? last = comparer == null
            ? AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>().LastOrDefault()
            : AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>().OrderBy(d => d, comparer).LastOrDefault();

        return last;
    }

    public TDrawable GetLastOrCreateNew<TDrawable>(IComparer<TDrawable>? comparer = null)
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable? drawable = GetLastOfType(comparer);
        return drawable ?? CreateNew<TDrawable>();
    }

    public void HideAll()
    {
        foreach (ImGuiDrawableBase drawable in AllDrawables.Where(d => !d.IsDisposed))
        {
            try { drawable.Hide(); }
            catch (NotSupportedException)
            {
                // ignore
            }
        }
    }

    public void HideAllOfType<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        foreach (TDrawable drawable in AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>())
        {
            try { drawable.Hide(); }
            catch (NotSupportedException)
            {
                // ignore   
            }
        }
    }

    public void ShowAll(bool show = true)
    {
        foreach (ImGuiDrawableBase drawable in AllDrawables.Where(d => !d.IsDisposed))
        {
            try { drawable.Show(show); }
            catch (NotSupportedException)
            {
                // ignore
            }
        }
    }

    public void ShowAllOfType<TDrawable>(bool show = true)
    where TDrawable : ImGuiDrawableBase
    {
        foreach (TDrawable drawable in AllDrawables.Where(d => !d.IsDisposed).OfType<TDrawable>())
        {
            try { drawable.Show(show); }
            catch (NotSupportedException)
            {
                // ignore
            }
        }
    }

    public bool TryCreateNew<TDrawable>(out TDrawable? drawable)
    where TDrawable : ImGuiDrawableBase
    {
        _logger.LogDebug("Attempting to create a new instance of type {DrawableType}", typeof(TDrawable));

        try { drawable = CreateNew_Impl<TDrawable>(); }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to create instance of {DrawableType}", typeof(TDrawable));
            drawable = null;
            return false;
        }

        if (AllDrawables.Contains(drawable))
        {
            _logger.LogDebug("The same instance is already present in drawables list");
            drawable = null;
            return false;
        }

        _modImGuiContext.AddDrawable(drawable);
        return true;
    }

    private TDrawable CreateNew_Impl<TDrawable>()
    where TDrawable : ImGuiDrawableBase
    {
        TDrawable drawable;
        try { drawable = ActivatorUtilities.GetServiceOrCreateInstance<TDrawable>(_serviceProvider); }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create instance of {DrawableType}, did you register all necessary service types?", typeof(TDrawable));
            throw;
        }

        return drawable;
    }
}