using System.Linq.Expressions;

namespace Interview.Domain.Repository;

public interface IMapper<TIn, TOut>
{
    Expression<Func<TIn, TOut>> Expression { get; }

    TOut Map(TIn input);
}

public static class Mapper<TIn>
{
#pragma warning disable CA1000
    public static Mapper<TIn, TOut> Create<TOut>(Expression<Func<TIn, TOut>> expression)
#pragma warning restore CA1000
    {
        return new Mapper<TIn, TOut>(expression);
    }
}
