using System;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Expression lhs, Term rhs)
    {
        Term[] terms = new Term[lhs._Terms.Length + 1];
        lhs._Terms.CopyTo(terms);
        terms[^1] = rhs;
        return new(terms, lhs.Constant);
    }

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    /// <summary>Subtracts the right expression value from the left expression value.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;
}
