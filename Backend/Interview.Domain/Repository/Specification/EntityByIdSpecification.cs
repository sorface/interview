using NSpecifications;

namespace Interview.Domain.Repository.Specification;

public sealed class EntityByIdSpecification<T> : Spec<T>
    where T : Entity
{
    public EntityByIdSpecification(Guid id)
        : base(e => e.Id == id)
    {
    }
}
