namespace Interview.Domain;

/// <summary>
/// The class represents an empty object implementing the <see cref="IDisposable"/> interface.
/// </summary>
public class EmptyDisposable : IDisposable
{
    /// <summary>
    /// Gets <see cref="EmptyDisposable"/> instance.
    /// </summary>
    public static readonly IDisposable Instance = new EmptyDisposable();

    private EmptyDisposable()
    {
    }

    /// <summary>
    /// Empry realization of dispose method.
    /// </summary>
    public void Dispose()
    {
    }
}
