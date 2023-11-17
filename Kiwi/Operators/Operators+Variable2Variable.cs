namespace Nanoray.Kiwi;

public partial record struct Variable
{
    public static Term operator -(Variable variable)
        => new(variable, -1);

    public static Expression operator +(Variable lhs, Variable rhs)
        => new(new Term[] { new(lhs), new(rhs) });

    public static Expression operator -(Variable lhs, Variable rhs)
        => lhs + -rhs;
}
