using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class ConstraintTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void ConstraintCreationEq()
    {
        Variable v = new("foo");
        Constraint c = new(v + 1.0, RelationalOperator.Equal);

        Assert.AreEqual(Strength.Required, c.Strength, Epsilon);
        Assert.AreEqual(RelationalOperator.Equal, c.Operator);

        Expression expr = c.Expression;
        Assert.AreEqual(1.0, expr.Constant, Epsilon);
        Assert.AreEqual(1, expr.Terms.Count);
        Assert.AreEqual(v, expr.Terms[0].Variable);
        Assert.AreEqual(1.0, expr.Terms[0].Coefficient, Epsilon);
    }

    [Test]
    public void ConstraintCreationLe()
    {
        Variable v = new("foo");
        Constraint c = new(v + 1.0, RelationalOperator.LessThanOrEqual);

        Assert.AreEqual(Strength.Required, c.Strength, Epsilon);
        Assert.AreEqual(RelationalOperator.LessThanOrEqual, c.Operator);
    }

    [Test]
    public void ConstraintCreationGe()
    {
        Variable v = new("foo");
        Constraint c = new(v + 1.0, RelationalOperator.GreaterThanOrEqual);

        Assert.AreEqual(Strength.Required, c.Strength, Epsilon);
        Assert.AreEqual(RelationalOperator.GreaterThanOrEqual, c.Operator);
    }

    [Test]
    public void ConstraintCreationWithStrength()
    {
        Variable v = new("foo");

        Constraint c1 = new(v + 1.0, RelationalOperator.Equal, Strength.Weak);
        Constraint c2 = new(v + 1.0, RelationalOperator.Equal, Strength.Medium);
        Constraint c3 = new(v + 1.0, RelationalOperator.Equal, Strength.Strong);
        Constraint c4 = new(v + 1.0, RelationalOperator.Equal, Strength.Required);

        Assert.AreEqual(Strength.Weak, c1.Strength, Epsilon);
        Assert.AreEqual(Strength.Medium, c2.Strength, Epsilon);
        Assert.AreEqual(Strength.Strong, c3.Strength, Epsilon);
        Assert.AreEqual(Strength.Required, c4.Strength, Epsilon);
    }

    [Test]
    public void ConstraintCloneWithStrength()
    {
        Variable v = new("foo");
        Constraint c = new(v + 1.0, RelationalOperator.Equal);

        Constraint c1 = new(c, Strength.Weak);
        Constraint c2 = new(c, Strength.Medium);
        Constraint c3 = new(c, Strength.Strong);
        Constraint c4 = new(c, Strength.Required);

        Assert.AreEqual(Strength.Weak, c1.Strength, Epsilon);
        Assert.AreEqual(Strength.Medium, c2.Strength, Epsilon);
        Assert.AreEqual(Strength.Strong, c3.Strength, Epsilon);
        Assert.AreEqual(Strength.Required, c4.Strength, Epsilon);
    }
}
