using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public sealed class Expression
{
    public IReadOnlyList<Term> Terms { get; set; }
    public double Constant { get; set; }

    public double Value
        => Terms.Sum(t => t.Value) + Constant;

    public bool IsConstant
        => Terms.Count == 0;

    public Expression(double constant = 0) : this(Array.Empty<Term>(), constant) { }

    public Expression(IReadOnlyList<Term> terms, double constant = 0)
    {
        this.Terms = terms;
        this.Constant = constant;
    }

    // TODO: ToString implementation if needed

    public static Expression operator -(Expression expression)
        => new(expression.Terms.Select(t => -t).ToList(), -expression.Constant);

    public static Expression operator +(Expression lhs, Expression rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + rhs.Terms.Count);
        terms.AddRange(lhs.Terms);
        terms.AddRange(rhs.Terms);
        return new(terms, lhs.Constant + rhs.Constant);
    }

    public static Expression operator +(Expression expression, double constant)
        => new(expression.Terms.ToList(), expression.Constant + constant);

    public static Expression operator +(double constant, Expression expression)
        => expression + constant;

    public static Expression operator +(Expression lhs, Term rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + 1);
        terms.AddRange(lhs.Terms);
        terms.Add(rhs);
        return new(terms, lhs.Constant);
    }

    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator +(Expression lhs, Variable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(Variable lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, Expression rhs)
        => lhs + -rhs;

    public static Expression operator -(Expression expression, double constant)
        => expression + -constant;

    public static Expression operator -(double constant, Expression expression)
        => constant + -expression;

    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;

    public static Expression operator -(Expression lhs, Variable rhs)
        => lhs + -rhs;

    public static Expression operator -(Variable lhs, Expression rhs)
        => lhs + -rhs;

    public static Expression operator *(Expression expression, double coefficient)
        => new(expression.Terms.Select(t => t * coefficient).ToList(), expression.Constant * coefficient);

    public static Expression operator *(double coefficient, Expression expression)
        => expression / coefficient;

    public static Expression operator /(Expression expression, double denominator)
        => new(expression.Terms.Select(t => t / denominator).ToList(), expression.Constant / denominator);

    public static Expression operator *(Expression lhs, Expression rhs)
    {
        if (lhs.IsConstant)
            return rhs * lhs.Constant;
        else if (rhs.IsConstant)
            return lhs * rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }

    public static Expression operator /(Expression lhs, Expression rhs)
    {
        if (rhs.IsConstant)
            return lhs / rhs.Constant;
        else
            throw new NonLinearExpressionException();
    }
}
