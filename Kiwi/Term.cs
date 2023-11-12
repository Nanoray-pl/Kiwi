using System.Collections.Generic;

namespace Nanoray.Kiwi;

public sealed class Term
{
    public Variable Variable { get; set; }
    public double Coefficient { get; set; }

    public double Value
        => Variable.Value * Coefficient;

    public Term(Variable variable, double coefficient = 1)
    {
        this.Variable = variable;
        this.Coefficient = coefficient;
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"{{Term: {Variable} x {Coefficient}}}";

    public static Term operator -(Term term)
        => new(term.Variable, -term.Coefficient);

    public static Expression operator +(Term lhs, Term rhs)
    {
        List<Term> terms = new() { lhs, rhs };
        return new(terms);
    }

    public static Expression operator +(Term lhs, Variable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(Variable lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator +(Term lhs, double rhs)
        => new(new List<Term> { lhs }, rhs);

    public static Expression operator +(double lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, Term rhs)
        => lhs + -rhs;

    public static Expression operator -(Term lhs, Variable rhs)
        => lhs + -rhs;

    public static Expression operator -(Variable lhs, Term rhs)
        => lhs + -rhs;

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
}
