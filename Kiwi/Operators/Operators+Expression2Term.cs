using System;

namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    public static Expression operator +(Expression lhs, Term rhs)
    {
        Term[] terms = new Term[lhs.Terms.Length + 1];
        Array.Copy(lhs.Terms, terms, lhs.Terms.Length);
        terms[^1] = rhs;
        return new(terms, lhs.Constant);
    }

    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;
}
