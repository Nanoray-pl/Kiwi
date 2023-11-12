using System.Collections.Generic;

namespace Nanoray.Kiwi;

public sealed class Variable : IVariable
{
    public string? Name { get; set; }
    public double Value { get; set; }

    public Variable(string? name = null, double value = 0)
    {
        this.Name = name;
        this.Value = value;
    }

    #region Variable-Variable operators
    public static Term operator -(Variable variable)
        => new(variable, -1);

    public static Expression operator +(IVariable lhs, Variable rhs)
    {
        List<Term> terms = new() { new(lhs), new(rhs) };
        return new(terms);
    }

    public static Expression operator -(IVariable lhs, Variable rhs)
        => lhs + -rhs;
    #endregion

    #region Variable-double operators
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
    #endregion
}
