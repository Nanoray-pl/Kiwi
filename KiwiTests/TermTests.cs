using System;
using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class TermTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void TermCreation()
    {
        Variable v = new("foo");

        Term t1 = new(v);
        Assert.AreEqual(v, t1.Variable);
        Assert.AreEqual(1.0, t1.Coefficient, Epsilon);

        Term t2 = new(v, 100.0);
        Assert.AreEqual(v, t2.Variable);
        Assert.AreEqual(100.0, t2.Coefficient, Epsilon);
    }

    [Test]
    public void TermNeg()
    {
        Variable v = new("foo");
        Term t = new(v, 10.0);

        Term neg = -t;
        Assert.AreEqual(v, neg.Variable);
        Assert.AreEqual(-10.0, neg.Coefficient, Epsilon);
    }

    [Test]
    public void TermMul()
    {
        Variable v = new("foo");
        Term t = new(v, 10.0);

        Term mul1 = t * 2.0;
        Assert.AreEqual(v, mul1.Variable);
        Assert.AreEqual(20.0, mul1.Coefficient, Epsilon);

        Term mul2 = 2.0 * t;
        Assert.AreEqual(v, mul2.Variable);
        Assert.AreEqual(20.0, mul2.Coefficient, Epsilon);
    }

    [Test]
    public void TermDiv()
    {
        Variable v = new("foo");
        Term t = new(v, 10.0);

        Term div = t / 2.0;
        Assert.AreEqual(v, div.Variable);
        Assert.AreEqual(5.0, div.Coefficient, Epsilon);
    }

    [Test]
    public void TermAdd()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Term t1 = new(v1, 10.0);
        Term t2 = new(v2);

        Expression add1 = t1 + 2.0;
        Assert.AreEqual(2.0, add1.Constant, Epsilon);
        Assert.AreEqual(1, add1.Terms.Count);
        Assert.AreEqual(v1, add1.Terms[0].Variable);
        Assert.AreEqual(10.0, add1.Terms[0].Coefficient, Epsilon);

        Expression add2 = 2.0 + t1;
        Assert.AreEqual(2.0, add2.Constant, Epsilon);
        Assert.AreEqual(1, add2.Terms.Count);
        Assert.AreEqual(v1, add2.Terms[0].Variable);
        Assert.AreEqual(10.0, add2.Terms[0].Coefficient, Epsilon);

        Expression add3 = t1 + v2;
        Assert.AreEqual(0.0, add3.Constant, Epsilon);
        Assert.AreEqual(2, add3.Terms.Count);
        Assert.AreEqual(v1, add3.Terms[0].Variable);
        Assert.AreEqual(10.0, add3.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add3.Terms[1].Variable);
        Assert.AreEqual(1.0, add3.Terms[1].Coefficient, Epsilon);

        Expression add4 = v2 + t1;
        Assert.AreEqual(0.0, add4.Constant, Epsilon);
        Assert.AreEqual(2, add4.Terms.Count);

        Expression add5 = t1 + t2;
        Assert.AreEqual(0.0, add5.Constant, Epsilon);
        Assert.AreEqual(2, add5.Terms.Count);
        Assert.AreEqual(v1, add5.Terms[0].Variable);
        Assert.AreEqual(10.0, add5.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add5.Terms[1].Variable);
        Assert.AreEqual(1.0, add5.Terms[1].Coefficient, Epsilon);
    }

    [Test]
    public void TermSub()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Term t1 = new(v1, 10.0);
        Term t2 = new(v2);

        Expression sub1 = t1 - 2.0;
        Assert.AreEqual(-2.0, sub1.Constant, Epsilon);
        Assert.AreEqual(1, sub1.Terms.Count);
        Assert.AreEqual(v1, sub1.Terms[0].Variable);
        Assert.AreEqual(10.0, sub1.Terms[0].Coefficient, Epsilon);

        Expression sub2 = 2.0 - t1;
        Assert.AreEqual(2.0, sub2.Constant, Epsilon);
        Assert.AreEqual(1, sub2.Terms.Count);
        Assert.AreEqual(v1, sub2.Terms[0].Variable);
        Assert.AreEqual(-10.0, sub2.Terms[0].Coefficient, Epsilon);

        Expression sub3 = t1 - v2;
        Assert.AreEqual(0.0, sub3.Constant, Epsilon);
        Assert.AreEqual(2, sub3.Terms.Count);
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 10.0) < Epsilon));
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));

        Expression sub4 = v2 - t1;
        Assert.AreEqual(0.0, sub4.Constant, Epsilon);
        Assert.AreEqual(2, sub4.Terms.Count);
        Assert.That(sub4.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient - 1.0) < Epsilon));
        Assert.That(sub4.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient + 10.0) < Epsilon));

        Expression sub5 = t1 - t2;
        Assert.AreEqual(0.0, sub5.Constant, Epsilon);
        Assert.AreEqual(2, sub5.Terms.Count);
        Assert.That(sub5.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 10.0) < Epsilon));
        Assert.That(sub5.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));
    }
}
