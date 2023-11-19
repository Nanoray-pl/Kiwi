namespace Nanoray.Kiwi;

public partial record struct Term
{
    /// <summary>Negates an expression value.</summary>
    /// <param name="value">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public static Term operator -(Term value)
        => new(value.Variable, -value.Coefficient);

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Term lhs, Term rhs)
        => new(new Term[] { lhs, rhs });

    /// <summary>Subtracts two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Term lhs, Term rhs)
        => lhs + -rhs;
}
