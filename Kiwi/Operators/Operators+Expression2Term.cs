using System.Collections.Generic;

namespace Nanoray.Kiwi;

public partial record struct Expression
{
    public static Expression operator +(Expression lhs, Term rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + 1);
        terms.AddRange(lhs.Terms);
        terms.Add(rhs);
        return new(terms, lhs.Constant);
    }

    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;
}
