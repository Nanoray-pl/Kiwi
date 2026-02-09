using System;
using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class VariableTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void VariableMethods()
    {
        Variable v = new Variable(name: null);
        Assert.IsNull(v.Name);
        Assert.AreEqual(0.0, v.Value, Epsilon);
        Assert.IsNotNull(v.Store);

        Variable named = new("foo");
        Assert.AreEqual("foo", named.Name);
    }

    [Test]
    public void VariableNeg()
    {
        Variable v = new("foo");

        Term neg = -v;
        Assert.AreEqual(v, neg.Variable);
        Assert.AreEqual(-1.0, neg.Coefficient, Epsilon);
    }

    [Test]
    public void VariableMul()
    {
        Variable v = new("foo");

        Term mul1 = v * 2.0;
        Assert.AreEqual(v, mul1.Variable);
        Assert.AreEqual(2.0, mul1.Coefficient, Epsilon);

        Term mul2 = 2.0 * v;
        Assert.AreEqual(v, mul2.Variable);
        Assert.AreEqual(2.0, mul2.Coefficient, Epsilon);
    }

    [Test]
    public void VariableDivision()
    {
        Variable v = new("foo");

        Term div = v / 2.0;
        Assert.AreEqual(v, div.Variable);
        Assert.AreEqual(0.5, div.Coefficient, Epsilon);
    }

    [Test]
    public void VariableAddition()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");

        Expression add1 = v1 + 2.0;
        Assert.AreEqual(2.0, add1.Constant, Epsilon);
        Assert.AreEqual(1, add1.Terms.Count);
        Assert.AreEqual(v1, add1.Terms[0].Variable);
        Assert.AreEqual(1.0, add1.Terms[0].Coefficient, Epsilon);

        Expression add2 = 2.0 + v1;
        Assert.AreEqual(2.0, add2.Constant, Epsilon);
        Assert.AreEqual(1, add2.Terms.Count);
        Assert.AreEqual(v1, add2.Terms[0].Variable);
        Assert.AreEqual(1.0, add2.Terms[0].Coefficient, Epsilon);

        Expression add3 = v1 + v2;
        Assert.AreEqual(0.0, add3.Constant, Epsilon);
        Assert.AreEqual(2, add3.Terms.Count);
        Assert.AreEqual(v1, add3.Terms[0].Variable);
        Assert.AreEqual(1.0, add3.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(v2, add3.Terms[1].Variable);
        Assert.AreEqual(1.0, add3.Terms[1].Coefficient, Epsilon);
    }

    [Test]
    public void VariableSubtraction()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");

        Expression sub1 = v1 - 2.0;
        Assert.AreEqual(-2.0, sub1.Constant, Epsilon);
        Assert.AreEqual(1, sub1.Terms.Count);
        Assert.AreEqual(v1, sub1.Terms[0].Variable);
        Assert.AreEqual(1.0, sub1.Terms[0].Coefficient, Epsilon);

        Expression sub2 = 2.0 - v1;
        Assert.AreEqual(2.0, sub2.Constant, Epsilon);
        Assert.AreEqual(1, sub2.Terms.Count);
        Assert.AreEqual(v1, sub2.Terms[0].Variable);
        Assert.AreEqual(-1.0, sub2.Terms[0].Coefficient, Epsilon);

        Expression sub3 = v1 - v2;
        Assert.AreEqual(0.0, sub3.Constant, Epsilon);
        Assert.AreEqual(2, sub3.Terms.Count);
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v1 && Math.Abs(t.Coefficient - 1.0) < Epsilon));
        Assert.That(sub3.Terms, Has.Exactly(1).Matches<Term>(t => t.Variable == v2 && Math.Abs(t.Coefficient + 1.0) < Epsilon));
    }
}
