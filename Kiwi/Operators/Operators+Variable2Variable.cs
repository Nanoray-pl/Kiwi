using System.Collections.Generic;

namespace Nanoray.Kiwi;

public sealed partial class Variable
{
    public static Term operator -(Variable variable)
        => new(variable, -1);

    public static Expression operator +(IVariable lhs, Variable rhs)
    {
        List<Term> terms = new() { new(lhs), new(rhs) };
        return new(terms);
    }

    public static Expression operator -(IVariable lhs, Variable rhs)
        => lhs + -rhs;
}
