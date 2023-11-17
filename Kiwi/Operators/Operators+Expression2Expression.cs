using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public partial record struct Expression
{
    public static Expression operator -(Expression expression)
        => new(expression.Terms.Select(t => -t).ToList(), -expression.Constant);

    public static Expression operator +(Expression lhs, Expression rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + rhs.Terms.Count);
        terms.AddRange(lhs.Terms);
        terms.AddRange(rhs.Terms);
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
