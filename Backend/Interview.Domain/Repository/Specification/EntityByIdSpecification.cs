using NSpecifications;

namespace Interview.Domain.Repository.Specification;

public sealed class EntityByIdSpecification<T>(Guid id) : Spec<T>(e => e.Id == id)
    where T : Entity;
