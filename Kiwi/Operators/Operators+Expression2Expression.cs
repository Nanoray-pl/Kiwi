using System;
using System.Linq;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    /// <summary>Negates an expression value.</summary>
    /// <param name="value">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public static Expression operator -(Expression value)
        => new(value._Terms.Select(t => -t).ToArray(), -value.Constant);

    /// <summary>Subtracts two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The subtracted expression.</returns>
    public static Expression operator +(Expression lhs, Expression rhs)
    {
        Term[] terms = new Term[lhs._Terms.Length + rhs._Terms.Length];
        Array.Copy(lhs._Terms, 0, terms, 0, lhs._Terms.Length);
        Array.Copy(rhs._Terms, 0, terms, lhs._Terms.Length, rhs._Terms.Length);
        return new(terms, lhs.Constant + rhs.Constant);
    }

    /// <summary>Sums two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The summed expression.</returns>
    public static Expression operator -(Expression lhs, Expression rhs)
        => lhs + -rhs;

    /// <summary>Multiplies two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The multiplied expression.</returns>
    public static Expression operator *(Expression lhs, Expression rhs)
    {
        if (lhs.IsConstant)
            return rhs * lhs.Constant;
        else if (rhs.IsConstant)
            return lhs * rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }

    /// <summary>Divides two expression values together.</summary>
    /// <param name="lhs">The left side of the expression.</param>
    /// <param name="rhs">The right side of the expression</param>
    /// <returns>The divides expression.</returns>
    public static Expression operator /(Expression lhs, Expression rhs)
    {
        if (rhs.IsConstant)
            return lhs / rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }
}
