namespace Nanoray.Kiwi;

public partial record struct Term
{
    public static Term operator -(Term term)
        => new(term.Variable, -term.Coefficient);

    public static Expression operator +(Term lhs, Term rhs)
        => new(new Term[] { lhs, rhs });

    public static Expression operator -(Term lhs, Term rhs)
        => lhs + -rhs;
}
