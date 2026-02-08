using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class SimplexTests
{
    [Test]
    public void Maximization()
    {
        Variable x1 = new("x1");
        Variable x2 = new("x2");
        Variable x3 = new("x3");
        Variable z = new("z");

        Solver solver = new();

        solver.AddConstraint(Constraint.Make(x1, RelationalOperator.GreaterThanOrEqual, 0.0));
        solver.AddConstraint(Constraint.Make(x2, RelationalOperator.GreaterThanOrEqual, 0.0));
        solver.AddConstraint(Constraint.Make(x3, RelationalOperator.GreaterThanOrEqual, 0.0));

        solver.AddConstraint(Constraint.Make(2.0 * x1 - 5.0 * x2, RelationalOperator.LessThanOrEqual, 11.0));
        solver.AddConstraint(Constraint.Make(-1.0 * x1 + 3.0 * x2 + x3, RelationalOperator.Equal, 7.0));
        solver.AddConstraint(Constraint.Make(x1 - 8.0 * x2 + 4.0 * x3, RelationalOperator.GreaterThanOrEqual, 33.0));

        solver.AddConstraint(Constraint.Make(z, RelationalOperator.Equal, -2.0 * x1 + 7.0 * x2 + 4.0 * x3));
        solver.AddEditVariable(z, Strength.Weak);
        solver.SuggestValue(z, 1e6);

        solver.Solve();

        Assert.AreEqual(13.0, x1.Value, 1e-4);
        Assert.AreEqual(3.0, x2.Value, 1e-4);
        Assert.AreEqual(11.0, x3.Value, 1e-4);
        Assert.AreEqual(39.0, z.Value, 1e-4);
    }
}
