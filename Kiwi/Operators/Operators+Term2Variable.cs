namespace Nanoray.Kiwi;

public partial record struct Term
{
    public static Expression operator +(Term lhs, IVariable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(IVariable lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, IVariable rhs)
        => lhs + rhs.Negate();

    public static Expression operator -(IVariable lhs, Term rhs)
        => lhs + -rhs;
}
