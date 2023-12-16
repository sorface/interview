namespace Interview.Domain.Repository;

/// <summary>
/// Repository of operations with an entity that is not archived.
/// </summary>
/// <typeparam name="T">Object type expanding from ArchiveEntity.</typeparam>
public interface INonArchiveRepository<T> : IRepository<T>
    where T : ArchiveEntity
{
}
