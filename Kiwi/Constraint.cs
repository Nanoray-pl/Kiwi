using System;
using System.Collections.Generic;

namespace Nanoray.Kiwi;

/// <summary>Describes a constraint placed on a number of variables in the solver system.</summary>
public readonly struct Constraint : IEquatable<Constraint>
{
    /// <summary>The expression held by the constraint.</summary>
    public Expression Expression { get; init; }

    /// <summary>The operator of the constraint.</summary>
    public RelationalOperator Operator { get; init; }

    /// <summary>The strength of the constraint (see <see cref="Strength"/>).</summary>
    public double Strength { get; init; }

    /// <summary>Create a constraint with the given expression.</summary>
    /// <param name="expression">The expression held by the constraint.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    public Constraint(Expression expression, RelationalOperator @operator, double? strength = null)
    {
        this.Expression = Reduce(expression);
        this.Operator = @operator;
        this.Strength = Kiwi.Strength.Clip(strength ?? Kiwi.Strength.Required);
    }

    /// <summary>Create a constraint cloning another constraint.</summary>
    /// <param name="other">The constraint to clone.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    public Constraint(Constraint other, double? strength = null) : this(other.Expression, other.Operator, strength) { }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
        => obj is Constraint constraint && Equals(constraint);

    /// <inheritdoc/>
    public readonly bool Equals(Constraint other)
        => Expression == other.Expression && Operator == other.Operator && Strength == other.Strength;

    /// <summary>The equality operator <c>==</c> returns <c>true</c> if its operands are equal, <c>false</c> otherwise.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if its operands are equal, <c>false</c> otherwise.</returns>
    public static bool operator ==(Constraint left, Constraint right)
        => left.Equals(right);

    /// <summary>The equality operator <c>!=</c> returns <c>false</c> if its operands are equal, <c>true</c> otherwise.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>false</c> if its operands are equal, <c>true</c> otherwise.</returns>
    public static bool operator !=(Constraint left, Constraint right)
        => !(left == right);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
        => (Expression, Operator, Strength).GetHashCode();

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Expression lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Expression lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Term lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Expression lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Variable lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Expression lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(double lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Term lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Term lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Variable lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Term lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(double lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Variable lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(Variable lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    /// <summary>Creates a constraint representing an equation/inequality.</summary>
    /// <param name="lhs">The left operand of an equation/inequality.</param>
    /// <param name="operator">The operator of the constraint.</param>
    /// <param name="rhs">The right operand of an equation/inequality.</param>
    /// <param name="strength">The strength of the constraint (see <see cref="Strength"/>). Defaults to <see cref="Strength.Required"/>.</param>
    /// <returns>A new constraint.</returns>
    public static Constraint Make(double lhs, RelationalOperator @operator, Variable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    private static Expression Reduce(Expression expr)
    {
        Dictionary<Variable, double> variables = new();
        foreach (var term in expr._Terms)
            variables[term.Variable] = variables.GetValueOrDefault(term.Variable) + term.Coefficient;

        int index = 0;
        Term[] reducedTerms = new Term[variables.Count];
        foreach (var (variable, value) in variables)
            reducedTerms[index++] = new Term(variable, value);
        return new(reducedTerms, expr.Constant);
    }
}
