using System.Linq.Expressions;

namespace Interview.Domain.Repository;

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
