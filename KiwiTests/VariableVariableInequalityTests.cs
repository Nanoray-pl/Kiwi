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

        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 100));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.LessThanOrEqual, y));
        solver.UpdateVariables();

        Assert.That(x.Value <= 100);

        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 90));
        solver.UpdateVariables();

        Assert.AreEqual(90, x.Value, Epsilon);
    }

    [Test]
    public void LessThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 100));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.LessThanOrEqual, y));
        solver.UpdateVariables();

        Assert.That(x.Value <= 100);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 110));
            solver.UpdateVariables();
        });
    }

    [Test]
    public void GreaterThanEqualTo()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 100));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.GreaterThanOrEqual, y));
        solver.UpdateVariables();

        Assert.That(x.Value >= 100);

        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 110));
        solver.UpdateVariables();

        Assert.AreEqual(110, x.Value, Epsilon);
    }

    [Test]
    public void GreaterThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 100));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.GreaterThanOrEqual, y));
        solver.UpdateVariables();

        Assert.That(x.Value >= 100);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 90));
            solver.UpdateVariables();
        });
    }
}
