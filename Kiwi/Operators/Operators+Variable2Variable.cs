namespace Nanoray.Kiwi;

public partial record struct Variable
{
    /// <summary>Negates an expression value.</summary>
    /// <param name="value">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public static Term operator -(Variable value)
        => new(value, -1);

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Variable lhs, Variable rhs)
        => new(new Term[] { new(lhs), new(rhs) });

    /// <summary>Subtracts two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Variable lhs, Variable rhs)
        => lhs + -rhs;
}
