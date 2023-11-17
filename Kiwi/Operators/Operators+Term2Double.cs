namespace Nanoray.Kiwi;

public partial record struct Term
{
    public static Expression operator +(Term lhs, double rhs)
        => new(lhs, rhs);

    public static Expression operator +(double lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, double rhs)
        => new(lhs, -rhs);

    public static Expression operator -(double lhs, Term rhs)
        => lhs + -rhs;

    public static Term operator *(Term term, double coefficient)
        => new(term.Variable, term.Coefficient * coefficient);

    public static Term operator *(double coefficient, Term term)
        => term * coefficient;

    public static Term operator /(Term term, double denominator)
        => new(term.Variable, term.Coefficient / denominator);
}
