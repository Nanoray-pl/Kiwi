using System;
using System.Linq;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    public static Expression operator -(Expression expression)
        => new(expression.Terms.Select(t => -t).ToArray(), -expression.Constant);

    public static Expression operator +(Expression lhs, Expression rhs)
    {
        Term[] terms = new Term[lhs.Terms.Length + rhs.Terms.Length];
        Array.Copy(lhs.Terms, 0, terms, 0, lhs.Terms.Length);
        Array.Copy(rhs.Terms, 0, terms, lhs.Terms.Length, rhs.Terms.Length);
        return new(terms, lhs.Constant + rhs.Constant);
    }

    public static Expression operator -(Expression lhs, Expression rhs)
        => lhs + -rhs;

    public static Expression operator *(Expression lhs, Expression rhs)
    {
        if (lhs.IsConstant)
            return rhs * lhs.Constant;
        else if (rhs.IsConstant)
            return lhs * rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }

    public static Expression operator /(Expression lhs, Expression rhs)
    {
        if (rhs.IsConstant)
            return lhs / rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }
}
