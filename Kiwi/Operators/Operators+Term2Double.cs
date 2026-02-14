namespace Nanoray.Kiwi;

public partial record struct Term
{
    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Term lhs, double rhs)
        => new(lhs, rhs);

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(double lhs, Term rhs)
        => new(rhs, lhs);

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Term lhs, double rhs)
        => new(lhs, -rhs);

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(double lhs, Term rhs)
        => lhs + -rhs;

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Term operator *(Term lhs, double rhs)
        => new(lhs.Variable, lhs.Coefficient * rhs);

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Term operator *(double lhs, Term rhs)
        => rhs * lhs;

    /// <summary>Divides the left expression value by the right value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The divided expression.</returns>
    public static Term operator /(Term lhs, double rhs)
        => new(lhs.Variable, lhs.Coefficient / rhs);
}
