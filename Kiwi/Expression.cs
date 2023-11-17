using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public partial record struct Expression(
    IReadOnlyList<Term> Terms,
    double Constant = 0
)
{
    public readonly double Value
        => Terms.Sum(t => t.Value) + Constant;

    public readonly bool IsConstant
        => Terms.Count == 0;

    public Expression(Term term, double constant = 0) : this(new List<Term> { term }, constant) { }

    public Expression(double constant = 0) : this(Array.Empty<Term>(), constant) { }
}
