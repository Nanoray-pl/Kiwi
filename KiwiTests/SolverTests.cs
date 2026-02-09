using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class SolverTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void SolverCreation()
    {
        Solver solver = new();
        Assert.IsNotNull(solver);
    }

    [Test]
    public void ManagingEditVariable()
    {
        Solver solver = new();
        Variable v1 = new("foo");
        Variable v2 = new("bar");

        Assert.IsFalse(solver.HasEditVariable(v1));
        solver.AddEditVariable(v1, Strength.Weak);
        Assert.IsTrue(solver.HasEditVariable(v1));

        Assert.Throws<DuplicateEditVariableException>(() => solver.AddEditVariable(v1, Strength.Medium));
        Assert.Throws<UnknownEditVariableException>(() => solver.RemoveEditVariable(v2));

        solver.RemoveEditVariable(v1);
        Assert.IsFalse(solver.HasEditVariable(v1));

        Assert.Throws<ArgumentException>(() => solver.AddEditVariable(v1, Strength.Required));

        solver.AddEditVariable(v2, Strength.Strong);
        Assert.IsTrue(solver.HasEditVariable(v2));

        Assert.Throws<UnknownEditVariableException>(() => solver.SuggestValue(v1, 10));
    }

    [Test]
    public void SuggestingValuesForEditVariables()
    {
        Solver solver = new();
        Variable v1 = new("foo");

        solver.AddEditVariable(v1, Strength.Medium);
        solver.AddConstraint(Constraint.Equal(v1, 1.0, Strength.Weak));
        solver.SuggestValue(v1, 2.0);
        solver.Solve();
        Assert.AreEqual(2.0, v1.Value, Epsilon);

        solver = new Solver();
        Variable v2 = new("bar");
        Variable v3 = new("baz");

        solver.AddEditVariable(v2, Strength.Weak);
        solver.AddConstraint(Constraint.Equal(v2 + v3, 0.0));
        solver.AddConstraint(Constraint.LessEqual(v2, -1.0));
        solver.AddConstraint(Constraint.GreaterEqual(v2, 0.0, Strength.Weak));
        solver.SuggestValue(v2, 0.0);
        solver.Solve();

        Assert.LessOrEqual(v2.Value, -1.0 + Epsilon);
    }

    [Test]
    public void ManagingConstraints()
    {
        Solver solver = new();
        Variable v = new("foo");
        Constraint c1 = Constraint.GreaterEqual(v, 1.0);
        Constraint c2 = Constraint.LessEqual(v, 0.0);

        Assert.IsFalse(solver.HasConstraint(c1));
        solver.AddConstraint(c1);
        Assert.IsTrue(solver.HasConstraint(c1));

        Assert.Throws<DuplicateConstraintException>(() => solver.AddConstraint(c1));
        Assert.Throws<UnknownConstraintException>(() => solver.RemoveConstraint(c2));
        Assert.Throws<UnsatisfiableConstraintException>(() => solver.AddConstraint(c2));

        solver.RemoveConstraint(c1);
        Assert.IsFalse(solver.HasConstraint(c1));

        solver.AddConstraint(c2);
        Assert.IsTrue(solver.HasConstraint(c2));
    }

    [Test]
    public void SolvingUnderConstrainedSystem()
    {
        Solver solver = new();
        Variable v = new("foo");
        Constraint c = Constraint.GreaterEqual(2.0 * v + 1.0, 0.0);

        solver.AddEditVariable(v, Strength.Weak);
        solver.AddConstraint(c);
        solver.SuggestValue(v, 10.0);
        solver.Solve();

        Assert.AreEqual(21.0, c.Expression.Value, 1.0e-6);
        Assert.AreEqual(20.0, c.Expression.Terms[0].Value, 1.0e-6);
        Assert.AreEqual(10.0, v.Value, 1.0e-6);
    }

    [Test]
    public void SolvingWithStrength()
    {
        Variable v1 = new("foo");
        Variable v2 = new("bar");
        Solver solver = new();

        solver.AddConstraint(Constraint.Equal(v1 + v2, 0.0));
        solver.AddConstraint(Constraint.Equal(v1, 10.0));
        solver.AddConstraint(Constraint.GreaterEqual(v2, 0.0, Strength.Weak));
        solver.Solve();

        Assert.AreEqual(10.0, v1.Value, 1.0e-6);
        Assert.AreEqual(-10.0, v2.Value, 1.0e-6);

        v1 = new Variable("foo");
        v2 = new Variable("bar");
        solver = new Solver();

        solver.AddConstraint(Constraint.Equal(v1 + v2, 0.0));
        solver.AddConstraint(Constraint.GreaterEqual(v1, 10.0, Strength.Medium));
        solver.AddConstraint(Constraint.Equal(v2, 2.0, Strength.Strong));
        solver.Solve();

        Assert.AreEqual(-2.0, v1.Value, 1.0e-6);
        Assert.AreEqual(2.0, v2.Value, 1.0e-6);
    }

    [Test]
    public void HandlingInfeasibleConstraints()
    {
        Variable xm = new("xm");
        Variable xl = new("xl");
        Variable xr = new("xr");
        Solver solver = new();

        solver.AddEditVariable(xm, Strength.Strong);
        solver.AddEditVariable(xl, Strength.Weak);
        solver.AddEditVariable(xr, Strength.Weak);
        solver.AddConstraint(Constraint.Equal(2.0 * xm, xl + xr));
        solver.AddConstraint(Constraint.LessEqual(xl + 20.0, xr));
        solver.AddConstraint(Constraint.GreaterEqual(xl, -10.0));
        solver.AddConstraint(Constraint.LessEqual(xr, 100.0));

        solver.SuggestValue(xm, 40.0);
        solver.SuggestValue(xr, 50.0);
        solver.SuggestValue(xl, 30.0);

        solver.SuggestValue(xm, 60.0);
        solver.SuggestValue(xm, 90.0);
        solver.Solve();

        Assert.AreEqual(2.0 * xm.Value, xl.Value + xr.Value, 1.0e-6);
        Assert.AreEqual(80.0, xl.Value, 1.0e-6);
        Assert.AreEqual(100.0, xr.Value, 1.0e-6);
    }

    [Test]
    public void ConstraintViolated()
    {
        Solver solver = new();
        Variable v = new("foo");

        Constraint c1 = Constraint.GreaterEqual(v, 10.0);
        Constraint c2 = Constraint.LessEqual(v, -5.0, Strength.Weak);

        solver.AddConstraint(c1);
        solver.AddConstraint(c2);
        solver.Solve();

        Assert.GreaterOrEqual(v.Value, 10.0);
        Assert.IsFalse(c1.Violated);
        Assert.IsTrue(c2.Violated);
    }
}
