using System.Linq.Expressions;

namespace Interview.Domain.Repository;

public interface IMapper<TIn, TOut>
{
    Expression<Func<TIn, TOut>> Expression { get; }

    TOut Map(TIn input);
}

public class Mapper<TIn, TOut> : IMapper<TIn, TOut>
{
    private readonly Lazy<Func<TIn, TOut>> _lazyFunc;

    public Expression<Func<TIn, TOut>> Expression { get; }

    public Mapper(Expression<Func<TIn, TOut>> expression)
    {
        Expression = expression;
        _lazyFunc = new Lazy<Func<TIn, TOut>>(() => Expression.Compile());
    }

    public TOut Map(TIn input) => _lazyFunc.Value(input);
}

public static class Mapper<TIn>
{
    public static Mapper<TIn, TOut> Create<TOut>(Expression<Func<TIn, TOut>> expression)
    {
        return new Mapper<TIn, TOut>(expression);
    }
}
