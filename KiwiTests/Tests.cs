using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class Tests
{
    private const double Epsilon = 1.0e-8;

    [Test]
    public void SimpleNew()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.AddConstraint(Constraint.Make(x + 2.0, RelationalOperator.Equal, 20.0));
        solver.UpdateVariables();

        Assert.AreEqual(18.0, x.Value, Epsilon);
    }

    [Test]
    public void Simple0()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 20.0));
        solver.AddConstraint(Constraint.Make(x + 2.0, RelationalOperator.Equal, y + 10.0));
        solver.UpdateVariables();

        Assert.AreEqual(20.0, x.Value, Epsilon);
        Assert.AreEqual(12.0, y.Value, Epsilon);
    }

    [Test]
    public void Simple1()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, y));
        solver.UpdateVariables();

        Assert.AreEqual(x.Value, y.Value, Epsilon);
    }

    [Test]
    public void Casso1()
    {
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        solver.AddConstraint(Constraint.Make(x, RelationalOperator.LessThanOrEqual, y));
        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, x + 3.0));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 10.0, Strength.Weak));
        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 10.0, Strength.Weak));
        solver.UpdateVariables();

        if (Math.Abs(x.Value - 10) < Epsilon)
        {
            Assert.AreEqual(10.0, x.Value, Epsilon);
            Assert.AreEqual(13.0, y.Value, Epsilon);
        }
        else
        {
            Assert.AreEqual(7.0, x.Value, Epsilon);
            Assert.AreEqual(10.0, y.Value, Epsilon);
        }
    }
}
