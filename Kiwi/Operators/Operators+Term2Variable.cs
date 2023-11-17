namespace Nanoray.Kiwi;

public partial record struct Term
{
    public static Expression operator +(Term lhs, Variable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(Variable lhs, Term rhs)
        => rhs + lhs;

    public static Expression operator -(Term lhs, Variable rhs)
        => lhs + -rhs;

    public static Expression operator -(Variable lhs, Term rhs)
        => lhs + -rhs;
}
