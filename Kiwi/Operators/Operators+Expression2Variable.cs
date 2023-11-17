namespace Nanoray.Kiwi;

public partial record struct Expression
{
    public static Expression operator +(Expression lhs, IVariable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(IVariable lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, IVariable rhs)
        => lhs + rhs.Negate();

    public static Expression operator -(IVariable lhs, Expression rhs)
        => lhs + -rhs;
}
