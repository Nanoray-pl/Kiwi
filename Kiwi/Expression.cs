using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public readonly partial struct Expression : IEquatable<Expression>
{
    internal readonly Term[] Terms;
    internal readonly double Constant;

    public readonly double Value
        => Terms.Sum(t => t.Value) + Constant;

    public readonly bool IsConstant
        => Terms.Length == 0;

    public Expression(Term[] terms, double constant = 0)
    {
        this.Terms = terms;
        this.Constant = constant;
    }

    public Expression(Term term, double constant = 0) : this(new Term[] { term }, constant) { }

    public Expression(double constant = 0) : this(Array.Empty<Term>(), constant) { }

    public override bool Equals(object? obj)
        => obj is Expression expression && Equals(expression);

    public bool Equals(Expression other)
    {
        if (this.Constant != other.Constant)
            return false;

        var thisCoefficients = GetVariableCoefficients();
        var otherCoefficients = other.GetVariableCoefficients();
        if (thisCoefficients.Count != otherCoefficients.Count)
            return false;
        if (thisCoefficients.Except(otherCoefficients).Any())
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = BitConverter.SingleToInt32Bits((float)this.Constant);
        foreach (var term in this.Terms)
            hash += term.GetHashCode();
        return hash;
    }

    public static bool operator ==(Expression left, Expression right)
        => left.Equals(right);

    public static bool operator !=(Expression left, Expression right)
        => !(left == right);

    private Dictionary<Variable, double> GetVariableCoefficients()
    {
        Dictionary<Variable, double> coefficients = new();
        foreach (var term in this.Terms)
            coefficients[term.Variable] = coefficients.GetValueOrDefault(term.Variable) + term.Coefficient;
        return coefficients;
    }
}
