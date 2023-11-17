namespace Nanoray.Kiwi;

public partial record struct Variable
{
    public static Expression operator +(Variable lhs, double rhs)
        => new(new Term(lhs), rhs);

    public static Expression operator +(double lhs, Variable rhs)
        => new(new Term(rhs), lhs);

    public static Expression operator -(Variable lhs, double rhs)
        => lhs + -rhs;

    public static Expression operator -(double lhs, Variable rhs)
        => lhs + -rhs;

    public static Term operator *(Variable variable, double coefficient)
        => new(variable, coefficient);

    public static Term operator *(double coefficient, Variable variable)
        => new(variable, coefficient);

    public static Term operator /(Variable variable, double denominator)
        => new(variable, 1 / denominator);
}
