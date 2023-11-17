using System.Collections.Generic;

namespace Nanoray.Kiwi;

public partial record struct Term
{
    public static Term operator -(Term term)
        => new(term.Variable, -term.Coefficient);

    public static Expression operator +(Term lhs, Term rhs)
    {
        List<Term> terms = new() { lhs, rhs };
        return new(terms);
    }

    public static Expression operator -(Term lhs, Term rhs)
        => lhs + -rhs;
}
