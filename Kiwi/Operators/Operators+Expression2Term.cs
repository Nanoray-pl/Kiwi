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
        Array.Copy(lhs._Terms, terms, lhs._Terms.Length);
        terms[^1] = rhs;
        return new(terms, lhs.Constant);
    }

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    /// <summary>Subtracts two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    /// <summary>Subtracts two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;
}
