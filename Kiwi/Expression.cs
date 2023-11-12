using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public record struct Expression(
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

    #region Expression-Expression operators
    public static Expression operator -(Expression expression)
        => new(expression.Terms.Select(t => -t).ToList(), -expression.Constant);

    public static Expression operator +(Expression lhs, Expression rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + rhs.Terms.Count);
        terms.AddRange(lhs.Terms);
        terms.AddRange(rhs.Terms);
        return new(terms, lhs.Constant + rhs.Constant);
    }

    public static Expression operator -(Expression lhs, Expression rhs)
        => lhs + -rhs;

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
    #endregion

    #region Expression-Term operators
    public static Expression operator +(Expression lhs, Term rhs)
    {
        List<Term> terms = new(lhs.Terms.Count + 1);
        terms.AddRange(lhs.Terms);
        terms.Add(rhs);
        return new(terms, lhs.Constant);
    }

    public static Expression operator +(Term lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, Term rhs)
        => lhs + -rhs;

    public static Expression operator -(Term lhs, Expression rhs)
        => lhs + -rhs;
    #endregion

    #region Expression-Variable operators
    public static Expression operator +(Expression lhs, IVariable rhs)
        => lhs + new Term(rhs);

    public static Expression operator +(IVariable lhs, Expression rhs)
        => rhs + lhs;

    public static Expression operator -(Expression lhs, IVariable rhs)
        => lhs + rhs.Negate();

    public static Expression operator -(IVariable lhs, Expression rhs)
        => lhs + -rhs;
    #endregion

    #region Expression-double operators
    public static Expression operator +(Expression expression, double constant)
        => new(expression.Terms.ToList(), expression.Constant + constant);

    public static Expression operator +(double constant, Expression expression)
        => expression + constant;

    public static Expression operator -(Expression expression, double constant)
        => expression + -constant;

    public static Expression operator -(double constant, Expression expression)
        => constant + -expression;

    public static Expression operator *(Expression expression, double coefficient)
        => new(expression.Terms.Select(t => t * coefficient).ToList(), expression.Constant * coefficient);

    public static Expression operator *(double coefficient, Expression expression)
        => expression / coefficient;

    public static Expression operator /(Expression expression, double denominator)
        => new(expression.Terms.Select(t => t / denominator).ToList(), expression.Constant / denominator);
    #endregion
}
