namespace Nanoray.Kiwi;

public readonly partial struct Expression
{
    public static Expression operator +(Expression lhs, Variable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(Variable lhs, Expression rhs)
        => rhs + new Term(lhs);

    public static Expression operator -(Expression lhs, Variable rhs)
        => lhs + -rhs;

    public static Expression operator -(Variable lhs, Expression rhs)
        => lhs + -rhs;
}
