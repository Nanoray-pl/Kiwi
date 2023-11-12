using System.Collections.Generic;

namespace Nanoray.Kiwi;

public record struct Term(
    IVariable Variable,
    double Coefficient = 1
)
{
    public readonly double Value
        => Variable.Value * Coefficient;

    #region Term-Term operators
    public static Term operator -(Term term)
        => new(term.Variable, -term.Coefficient);

    public static Expression operator +(Term lhs, Term rhs)
    {
        List<Term> terms = new() { lhs, rhs };
        return new(terms);
    }

    public static Expression operator -(Term lhs, Term rhs)
        => lhs + -rhs;
    #endregion

    #region Term-Variable operators
    public static Expression operator +(Term lhs, IVariable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(IVariable lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, IVariable rhs)
        => lhs + rhs.Negate();

    public static Expression operator -(IVariable lhs, Term rhs)
        => lhs + -rhs;
    #endregion

    #region Term-double operators
    public static Expression operator +(Term lhs, double rhs)
        => new(new List<Term> { lhs }, rhs);

    public static Expression operator +(double lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, double rhs)
        => new(new List<Term> { lhs }, -rhs);

    public static Expression operator -(double lhs, Term rhs)
        => lhs + -rhs;

    public static Term operator *(Term term, double coefficient)
        => new(term.Variable, term.Coefficient * coefficient);

    public static Term operator *(double coefficient, Term term)
        => term * coefficient;

    public static Term operator /(Term term, double denominator)
        => new(term.Variable, term.Coefficient / denominator);
    #endregion
}
