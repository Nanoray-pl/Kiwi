using System.Linq;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    public static Expression operator +(Expression expression, double constant)
        => new(expression.Terms, expression.Constant + constant);

    public static Expression operator +(double constant, Expression expression)
        => expression + constant;

    public static Expression operator -(Expression expression, double constant)
        => expression + -constant;

    public static Expression operator -(double constant, Expression expression)
        => constant + -expression;

    public static Expression operator *(Expression expression, double coefficient)
        => new(expression.Terms.Select(t => t * coefficient).ToArray(), expression.Constant * coefficient);

    public static Expression operator *(double coefficient, Expression expression)
        => expression / coefficient;

    public static Expression operator /(Expression expression, double denominator)
        => new(expression.Terms.Select(t => t / denominator).ToArray(), expression.Constant / denominator);
}
