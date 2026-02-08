using System;
using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class ExpressionTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void ExpressionCreation()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Variable v3 = new("aux");

        Expression e1 = new(new[] { new Term(v1, 1), new Term(v2, 2), new Term(v3, 3) });
        Expression e2 = new(new[] { new Term(v1, 1), new Term(v2, 2), new Term(v3, 3) }, 10.0);

        Assert.AreEqual(0.0, e1.Constant, Epsilon);
        Assert.AreEqual(3, e1.Terms.Count);
        Assert.AreEqual(v1, e1.Terms[0].Variable);
        Assert.AreEqual(1.0, e1.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, e1.Terms[1].Variable);
        Assert.AreEqual(2.0, e1.Terms[1].Coefficient, Epsilon);
        Assert.AreEqual(v3, e1.Terms[2].Variable);
        Assert.AreEqual(3.0, e1.Terms[2].Coefficient, Epsilon);

        Assert.AreEqual(10.0, e2.Constant, Epsilon);
        Assert.AreEqual(3, e2.Terms.Count);
        Assert.AreEqual(v1, e2.Terms[0].Variable);
        Assert.AreEqual(1.0, e2.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, e2.Terms[1].Variable);
        Assert.AreEqual(2.0, e2.Terms[1].Coefficient, Epsilon);
        Assert.AreEqual(v3, e2.Terms[2].Variable);
        Assert.AreEqual(3.0, e2.Terms[2].Coefficient, Epsilon);
    }

    [Test]
    public void ExpressionNeg()
    {
        Variable v = new("foo");
        Expression e = new(new Term(v, 10.0), 5.0);

        Expression neg = -e;
        Assert.AreEqual(-5.0, neg.Constant, Epsilon);
        Assert.AreEqual(1, neg.Terms.Count);
        Assert.AreEqual(v, neg.Terms[0].Variable);
        Assert.AreEqual(-10.0, neg.Terms[0].Coefficient, Epsilon);
    }

    [Test]
    public void ExpressionMul()
    {
        Variable v = new("foo");
        Expression e = new(new Term(v, 10.0), 5.0);

        Expression mul1 = e * 2.0;
        Assert.AreEqual(10.0, mul1.Constant, Epsilon);
        Assert.AreEqual(1, mul1.Terms.Count);
        Assert.AreEqual(v, mul1.Terms[0].Variable);
        Assert.AreEqual(20.0, mul1.Terms[0].Coefficient, Epsilon);

        Expression mul2 = 2.0 * e;
        Assert.AreEqual(10.0, mul2.Constant, Epsilon);
        Assert.AreEqual(1, mul2.Terms.Count);
        Assert.AreEqual(v, mul2.Terms[0].Variable);
        Assert.AreEqual(20.0, mul2.Terms[0].Coefficient, Epsilon);
    }

    [Test]
    public void ExpressionDiv()
    {
        Variable v = new("foo");
        Expression e = new(new Term(v, 10.0), 5.0);

        Expression div = e / 2.0;
        Assert.AreEqual(2.5, div.Constant, Epsilon);
        Assert.AreEqual(1, div.Terms.Count);
        Assert.AreEqual(v, div.Terms[0].Variable);
        Assert.AreEqual(5.0, div.Terms[0].Coefficient, Epsilon);
    }

    [Test]
    public void ExpressionAddition()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Term t1 = new(v1, 10.0);
        Term t2 = new(v2);
        Expression e1 = t1 + 5.0;
        Expression e2 = v2 - 10.0;

        Expression add1 = e1 + 2.0;
        Assert.AreEqual(7.0, add1.Constant, Epsilon);
        Assert.AreEqual(1, add1.Terms.Count);
        Assert.AreEqual(v1, add1.Terms[0].Variable);
        Assert.AreEqual(10.0, add1.Terms[0].Coefficient, Epsilon);

        Expression add2 = 2.0 + e1;
        Assert.AreEqual(7.0, add2.Constant, Epsilon);
        Assert.AreEqual(1, add2.Terms.Count);
        Assert.AreEqual(v1, add2.Terms[0].Variable);
        Assert.AreEqual(10.0, add2.Terms[0].Coefficient, Epsilon);

        Expression add3 = e1 + v2;
        Assert.AreEqual(5.0, add3.Constant, Epsilon);
        Assert.AreEqual(2, add3.Terms.Count);
        Assert.AreEqual(v1, add3.Terms[0].Variable);
        Assert.AreEqual(10.0, add3.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add3.Terms[1].Variable);
        Assert.AreEqual(1.0, add3.Terms[1].Coefficient, Epsilon);

        Expression add4 = e1 + t2;
        Assert.AreEqual(5.0, add4.Constant, Epsilon);
        Assert.AreEqual(2, add4.Terms.Count);
        Assert.AreEqual(v1, add4.Terms[0].Variable);
        Assert.AreEqual(10.0, add4.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add4.Terms[1].Variable);
        Assert.AreEqual(1.0, add4.Terms[1].Coefficient, Epsilon);

        Expression add5 = e1 + e2;
        Assert.AreEqual(-5.0, add5.Constant, Epsilon);
        Assert.AreEqual(2, add5.Terms.Count);
        Assert.AreEqual(v1, add5.Terms[0].Variable);
        Assert.AreEqual(10.0, add5.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add5.Terms[1].Variable);
        Assert.AreEqual(1.0, add5.Terms[1].Coefficient, Epsilon);
    }

    [Test]
    public void ExpressionSubtraction()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Term t1 = new(v1, 10.0);
        Term t2 = new(v2);
        Expression e1 = t1 + 5.0;
        Expression e2 = v2 - 10.0;

        Expression sub1 = e1 - 2.0;
        Assert.AreEqual(3.0, sub1.Constant, Epsilon);
        Assert.AreEqual(1, sub1.Terms.Count);
        Assert.AreEqual(v1, sub1.Terms[0].Variable);
        Assert.AreEqual(10.0, sub1.Terms[0].Coefficient, Epsilon);

        Expression sub2 = 2.0 - e1;
        Assert.AreEqual(-3.0, sub2.Constant, Epsilon);
        Assert.AreEqual(1, sub2.Terms.Count);
        Assert.AreEqual(v1, sub2.Terms[0].Variable);
        Assert.AreEqual(-10.0, sub2.Terms[0].Coefficient, Epsilon);

        Expression sub3 = e1 - v2;
        Assert.AreEqual(5.0, sub3.Constant, Epsilon);
        Assert.AreEqual(2, sub3.Terms.Count);
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 10.0) < Epsilon));
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));

        Expression sub4 = v2 - e1;
        Assert.AreEqual(-5.0, sub4.Constant, Epsilon);
        Assert.AreEqual(2, sub4.Terms.Count);
        Assert.That(sub4.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient - 1.0) < Epsilon));
        Assert.That(sub4.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient + 10.0) < Epsilon));

        Expression sub5 = e1 - t2;
        Assert.AreEqual(5.0, sub5.Constant, Epsilon);
        Assert.AreEqual(2, sub5.Terms.Count);
        Assert.That(sub5.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 10.0) < Epsilon));
        Assert.That(sub5.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));

        Expression sub6 = t2 - e1;
        Assert.AreEqual(-5.0, sub6.Constant, Epsilon);
        Assert.AreEqual(2, sub6.Terms.Count);

        Expression sub7 = e1 - e2;
        Assert.AreEqual(15.0, sub7.Constant, Epsilon);
        Assert.AreEqual(2, sub7.Terms.Count);
        Assert.That(sub7.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 10.0) < Epsilon));
        Assert.That(sub7.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));
    }
}
