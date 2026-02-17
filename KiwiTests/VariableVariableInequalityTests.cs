using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class VariableVariableInequalityTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void LessThanEqualTo()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Equal(y, 100));
        solver.AddConstraint(Constraint.LessEqual(x, y));
        solver.Solve();

        Assert.That(x.Value <= 100);

        solver.AddConstraint(Constraint.Equal(x, 90));
        solver.Solve();

        Assert.AreEqual(90, x.Value, Epsilon);
    }

    [Test]
    public void LessThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Equal(y, 100));
        solver.AddConstraint(Constraint.LessEqual(x, y));
        solver.Solve();

        Assert.That(x.Value <= 100);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Equal(x, 110));
        });
    }

    [Test]
    public void GreaterThanEqualTo()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Equal(y, 100));
        solver.AddConstraint(Constraint.GreaterEqual(x, y));
        solver.Solve();

        Assert.That(x.Value >= 100);

        solver.AddConstraint(Constraint.Equal(x, 110));
        solver.Solve();

        Assert.AreEqual(110, x.Value, Epsilon);
    }

    [Test]
    public void GreaterThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Equal(y, 100));
        solver.AddConstraint(Constraint.GreaterEqual(x, y));
        solver.Solve();

        Assert.That(x.Value >= 100);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Equal(x, 90));
        });
    }
}
