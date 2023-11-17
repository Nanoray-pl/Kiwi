using System;
using System.Collections.Generic;

namespace Nanoray.Kiwi;

public readonly struct Constraint : IEquatable<Constraint>
{
    public Expression Expression { get; init; }
    public RelationalOperator Operator { get; init; }
    public double Strength { get; init; }

    public Constraint(Expression expression, RelationalOperator @operator, double? strength = null)
    {
        this.Expression = Reduce(expression);
        this.Operator = @operator;
        this.Strength = Kiwi.Strength.Clip(strength ?? Kiwi.Strength.Required);
    }

    public Constraint(Constraint other, double? strength = null) : this(other.Expression, other.Operator, strength) { }

    public override readonly bool Equals(object? obj)
        => obj is Constraint constraint && Equals(constraint);

    public readonly bool Equals(Constraint other)
        => Expression == other.Expression && Operator == other.Operator && Strength == other.Strength;

    public static bool operator ==(Constraint left, Constraint right)
        => left.Equals(right);

    public static bool operator !=(Constraint left, Constraint right)
        => !(left == right);

    public override readonly int GetHashCode()
        => (Expression, Operator, Strength).GetHashCode();

    public static Constraint Make(Expression lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Variable lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Variable lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Variable lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Variable lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    private static Expression Reduce(Expression expr)
    {
        Dictionary<Variable, double> variables = new();
        foreach (var term in expr.Terms)
            variables[term.Variable] = variables.GetValueOrDefault(term.Variable) + term.Coefficient;

        int index = 0;
        Term[] reducedTerms = new Term[variables.Count];
        foreach (var (variable, value) in variables)
            reducedTerms[index++] = new Term(variable, value);
        return new(reducedTerms, expr.Constant);
    }
}
