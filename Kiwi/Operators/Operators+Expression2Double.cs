using System.Collections.Immutable;
using System.Linq;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Expression lhs, double rhs)
        => new(lhs._Terms, lhs.Constant + rhs);

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(double lhs, Expression rhs)
        => rhs + lhs;

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Expression lhs, double rhs)
        => lhs + -rhs;

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(double lhs, Expression rhs)
        => lhs + -rhs;

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Expression operator *(Expression lhs, double rhs)
        => new(lhs._Terms.Select(t => t * rhs).ToImmutableArray(), lhs.Constant * rhs);

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Expression operator *(double lhs, Expression rhs)
        => rhs * lhs;

    /// <summary>Divides the left expression value by the right value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The divided expression.</returns>
    public static Expression operator /(Expression lhs, double rhs)
        => new(lhs._Terms.Select(t => t / rhs).ToImmutableArray(), lhs.Constant / rhs);
}
