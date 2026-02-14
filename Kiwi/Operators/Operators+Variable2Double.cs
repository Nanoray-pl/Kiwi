namespace Nanoray.Kiwi;

public sealed partial class Variable
{
    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Variable lhs, double rhs)
        => new(new Term(lhs), rhs);

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(double lhs, Variable rhs)
        => new(new Term(rhs), lhs);

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Variable lhs, double rhs)
        => lhs + -rhs;

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(double lhs, Variable rhs)
        => lhs + -rhs;

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Term operator *(Variable lhs, double rhs)
        => new(lhs, rhs);

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Term operator *(double lhs, Variable rhs)
        => new(rhs, lhs);

    /// <summary>Divides the left expression value by the right value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The divided expression.</returns>
    public static Term operator /(Variable lhs, double rhs)
        => new(lhs, 1 / rhs);
}
