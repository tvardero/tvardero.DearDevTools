using Microsoft.Extensions.ObjectPool;

namespace ta.UIKit.Inputs;

public record InputEvent : IResettable
{
    public bool IsAlreadyHandled { get; private set; }

    public void SetHandled()
    {
        IsAlreadyHandled = true;
    }

    /// <inheritdoc />
    public virtual bool TryReset()
    {
        IsAlreadyHandled = false;

        return true;
    }
}
