using System.Collections.Generic;

namespace Nanoray.Kiwi;

public sealed class Constraint
{
    public Expression Expression { get; set; }
    public RelationalOperator Operator { get; set; }
    public double Strength { get; set; }

    public Constraint(Expression expression, RelationalOperator @operator, double? strength = null)
    {
        this.Expression = Reduce(expression);
        this.Operator = @operator;
        this.Strength = Kiwi.Strength.Clip(strength ?? Kiwi.Strength.Required);
    }

    public Constraint(Constraint other, double? strength = null) : this(other.Expression, other.Operator, strength) { }

    /// <inheritdoc/>
    public override string ToString()
        => $"{{Expression {Expression}, Operator {Operator}, Strength {Strength}}}";

    public static Constraint Make(Expression lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, IVariable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(IVariable lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Expression lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, Expression rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, IVariable rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(IVariable lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(Term lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, Term rhs, double? strength = null)
        => new(lhs - rhs, @operator, strength);

    public static Constraint Make(IVariable lhs, RelationalOperator @operator, IVariable rhs, double? strength = null)
        => new(lhs.Subtract(rhs), @operator, strength);

    public static Constraint Make(IVariable lhs, RelationalOperator @operator, double rhs, double? strength = null)
        => new(lhs.Subtract(rhs), @operator, strength);

    public static Constraint Make(double lhs, RelationalOperator @operator, IVariable rhs, double? strength = null)
        => new(rhs.Negate() + lhs, @operator, strength);

    private static Expression Reduce(Expression expr)
    {
        Dictionary<IVariable, double> variables = new();
        foreach (var term in expr.Terms)
        {
            if (!variables.TryGetValue(term.Variable, out double value))
                value = 0;
            variables[term.Variable] = value + term.Coefficient;
        }

        List<Term> reducedTerms = new(variables.Count);
        foreach (var (variable, value) in variables)
            reducedTerms.Add(new Term(variable, value));
        return new(reducedTerms, expr.Constant);
    }
}
