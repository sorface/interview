using System.Linq.Expressions;

namespace Interview.Domain.Repository;

public interface IMapper<TIn, TOut>
{
    Expression<Func<TIn, TOut>> Expression { get; }

    TOut Map(TIn input);
}

public static class Mapper<TIn>
{
    public static Mapper<TIn, TOut> Create<TOut>(Expression<Func<TIn, TOut>> expression)
    {
        return new Mapper<TIn, TOut>(expression);
    }
}
