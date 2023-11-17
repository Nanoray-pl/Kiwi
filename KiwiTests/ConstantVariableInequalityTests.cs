using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class ConstantVariableInequalityTests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void LessThanEqualTo()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(100, RelationalOperator.LessThanOrEqual, x));
        });

        Assert.That(100 <= x.Value);

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 110));
        });

        Assert.AreEqual(110, x.Value, Epsilon);
    }

    [Test]
    public void LessThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(100, RelationalOperator.LessThanOrEqual, x));
        });

        Assert.That(100 <= x.Value);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.WithTransaction(transaction =>
            {
                transaction.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 10));
            });
        });
    }

    [Test]
    public void GreaterThanEqualTo()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(100, RelationalOperator.GreaterThanOrEqual, x));
        });

        Assert.That(100 >= x.Value);

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 90));
        });

        Assert.AreEqual(90, x.Value, Epsilon);
    }

    [Test]
    public void GreaterThanEqualToUnsatisfiable()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.WithTransaction(transaction =>
        {
            transaction.AddConstraint(Constraint.Make(100, RelationalOperator.GreaterThanOrEqual, x));
        });

        Assert.That(100 >= x.Value);

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.WithTransaction(transaction =>
            {
                transaction.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 110));
            });
        });
    }
}
