namespace Nanoray.Kiwi;

public sealed partial class Variable
{
    public static Expression operator +(Variable lhs, double rhs)
        => ((IVariable)lhs).Add(rhs);

    public static Expression operator +(double lhs, Variable rhs)
        => rhs + lhs;

    public static Expression operator -(Variable lhs, double rhs)
        => ((IVariable)lhs).Subtract(rhs);

    public static Expression operator -(double lhs, Variable rhs)
        => lhs + -rhs;

    public static Term operator *(Variable variable, double coefficient)
        => new(variable, coefficient);

    public static Term operator *(double coefficient, Variable variable)
        => variable * coefficient;

    public static Term operator /(Variable variable, double denominator)
        => new(variable, 1 / denominator);
}
