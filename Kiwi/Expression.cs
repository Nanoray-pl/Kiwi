using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

/// <summary>Describes a full expression consisting of variable components and a constant.</summary>
public readonly partial struct Expression : IEquatable<Expression>
{
    /// <summary>The terms, or in other words, variable components.</summary>
    public IReadOnlyList<Term> Terms
        => this._Terms;

    /// <summary>The constant added to the result.</summary>
    public readonly double Constant;

    internal readonly Term[] _Terms;

    /// <summary>The sum of all variable components' values and the constant.</summary>
    public double Value
        => _Terms.Sum(t => t.Value) + Constant;

    /// <summary>Whether the expression is constant, as in, it does not depend on any variables.</summary>
    public bool IsConstant
        => this._Terms.Length == 0;

    internal Expression(Term[] terms, double constant = 0)
    {
        this._Terms = terms;
        this.Constant = constant;
    }

    /// <summary>Create an expression with the given terms.</summary>
    /// <param name="terms">The terms, or in other words, variable components.</param>
    /// <param name="constant">The constant added to the result.</param>
    public Expression(IReadOnlyCollection<Term> terms, double constant = 0) : this(terms.ToArray(), constant) { }

    /// <summary>Create an expression with the given term.</summary>
    /// <param name="term">The term, or in other words, variable component.</param>
    /// <param name="constant">The constant added to the result.</param>
    public Expression(Term term, double constant = 0) : this(new Term[] { term }, constant) { }

    /// <summary>Create a constant expression.</summary>
    /// <param name="constant">The result constant.</param>
    public Expression(double constant = 0) : this(Array.Empty<Term>(), constant) { }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Expression expression && Equals(expression);

    /// <inheritdoc/>
    public bool Equals(Expression other)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (this.Constant != other.Constant)
            return false;

        var thisCoefficients = GetVariableCoefficients();
        var otherCoefficients = other.GetVariableCoefficients();
        if (thisCoefficients.Count != otherCoefficients.Count)
            return false;

        foreach (var thisCoefficient in thisCoefficients)
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (!otherCoefficients.TryGetValue(thisCoefficient.Key, out double otherValue) || thisCoefficient.Value != otherValue)
                return false;

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = BitConverter.SingleToInt32Bits((float)this.Constant);
        foreach (var coefficient in this.GetVariableCoefficients())
            hash += coefficient.Key.GetHashCode() ^ coefficient.Value.GetHashCode();
        return hash;
    }

    /// <summary>The equality operator <c>==</c> returns <c>true</c> if its operands are equal, <c>false</c> otherwise.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if its operands are equal, <c>false</c> otherwise.</returns>
    public static bool operator ==(Expression left, Expression right)
        => left.Equals(right);

    /// <summary>The equality operator <c>!=</c> returns <c>false</c> if its operands are equal, <c>true</c> otherwise.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>false</c> if its operands are equal, <c>true</c> otherwise.</returns>
    public static bool operator !=(Expression left, Expression right)
        => !(left == right);

    /// <summary>Implicitly converts a <see cref="Term"/> to an <see cref="Expression"/>.</summary>
    public static implicit operator Expression(Term term)
        => new(term);

    /// <summary>Implicitly converts a <see cref="Variable"/> to an <see cref="Expression"/>.</summary>
    public static implicit operator Expression(Variable variable)
        => new(new Term(variable));

    /// <summary>Implicitly converts a <see cref="double"/> to a constant <see cref="Expression"/>.</summary>
    public static implicit operator Expression(double constant)
        => new(constant);

    private Dictionary<Variable, double> GetVariableCoefficients()
    {
        Dictionary<Variable, double> coefficients = new();
        foreach (var term in this._Terms)
            coefficients[term.Variable] = coefficients.GetValueOrDefault(term.Variable) + term.Coefficient;
        return coefficients;
    }
}
