using System.Collections.Generic;

namespace Nanoray.Kiwi;

public interface IVariable
{
    double Value { get; set; }

    Term Negate()
        => new(this, -1);

    Expression Add(IVariable other)
    {
        List<Term> terms = new() { new(this), new(other) };
        return new(terms);
    }

    Expression Add(double value)
        => new Term(this) + value;

    Expression Subtract(IVariable other)
        => this + other.Negate();

    Expression Subtract(double value)
        => Add(-value);
}
