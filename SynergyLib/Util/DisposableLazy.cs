using System;

namespace SynergyLib.Util; 

public sealed class DisposableLazy<T> : IDisposable where T : IDisposable? {
    public readonly Lazy<T> Lazy;
    
    public DisposableLazy(Lazy<T> lazy) {
        Lazy = lazy;
    }

    public T Value => Lazy.Value;

    public void Dispose() {
        if (Lazy.IsValueCreated)
            Value?.Dispose();
    }
}
