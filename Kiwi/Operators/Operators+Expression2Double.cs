using System.Linq;

namespace Nanoray.Kiwi;

public partial record struct Expression
{
    public static Expression operator +(Expression expression, double constant)
        => new(expression.Terms.ToList(), expression.Constant + constant);

    public static Expression operator +(double constant, Expression expression)
        => expression + constant;

    public static Expression operator -(Expression expression, double constant)
        => expression + -constant;

    public static Expression operator -(double constant, Expression expression)
        => constant + -expression;

    public static Expression operator *(Expression expression, double coefficient)
        => new(expression.Terms.Select(t => t * coefficient).ToList(), expression.Constant * coefficient);

    public static Expression operator *(double coefficient, Expression expression)
        => expression / coefficient;

    public static Expression operator /(Expression expression, double denominator)
        => new(expression.Terms.Select(t => t / denominator).ToList(), expression.Constant / denominator);
}
