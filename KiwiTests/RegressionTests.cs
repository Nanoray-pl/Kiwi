using Nanoray.Kiwi;
using NUnit.Framework;

namespace KiwiTests;

/// <summary>
/// Regression tests for issues found by comparing the C# port against the
/// original C++ Kiwi source (https://github.com/nucleic/kiwi).
/// These tests should fail if the regressions reappear.
/// </summary>
public sealed class RegressionTests
{
    private const double Epsilon = 1.0e-8;

    // =========================================================================
    // double * Expression should multiply, not divide
    // =========================================================================

    [Test]
    public void DoubleTimesExpression_ShouldMultiply()
    {
        Variable x = new("x");
        Expression expr = new(new Term(x), 10.0); // x + 10

        // 2.0 * (x + 10) should equal (2x + 20), not ((x + 10) / 2) = (0.5x + 5)
        Expression result = 2.0 * expr;

        // The result should have coefficient 2.0 on x and constant 20.0
        Assert.AreEqual(1, result.Terms.Count, "Should have exactly one term");
        Assert.AreEqual(2.0, result.Terms[0].Coefficient, Epsilon, "Coefficient should be 2.0 (multiplied), not 0.5 (divided)");
        Assert.AreEqual(20.0, result.Constant, Epsilon, "Constant should be 20.0 (multiplied), not 5.0 (divided)");
    }

    [Test]
    public void DoubleTimesExpression_InSolver()
    {
        // Verify the bug manifests in actual constraint solving
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        // Set x = 10, then y = 2 * x (using double * Variable, which goes through double * Expression)
        solver.WithTransaction(s =>
        {
            s.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 10.0));
            // 2.0 * x uses: double * Variable → double * Term → double * Expression
            s.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 2.0 * x));
        });

        Assert.AreEqual(10.0, x.Value, Epsilon);
        Assert.AreEqual(20.0, y.Value, Epsilon, "y should be 2*x = 20, not x/2 = 5");
    }

    [Test]
    public void ExpressionTimesDouble_ShouldWork()
    {
        // Verify the workaround (Expression * double) is correct
        Variable x = new("x");
        Expression expr = new(new Term(x), 10.0); // x + 10

        Expression result = expr * 2.0;

        Assert.AreEqual(1, result.Terms.Count);
        Assert.AreEqual(2.0, result.Terms[0].Coefficient, Epsilon);
        Assert.AreEqual(20.0, result.Constant, Epsilon);
    }

    // =========================================================================
    // DualOptimize should fix infeasible rows
    // =========================================================================

    [Test]
    public void DualOptimize_ShouldFixInfeasibleRows()
    {
        // DualOptimize is called by Solve(). It's needed when rows become
        // infeasible after SuggestValue or constraint removal.
        Solver solver = new();
        Variable x = new("x");

        // x >= 10 (required)
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.GreaterThanOrEqual, 10.0));

        // x == 50 (weak — should be satisfied when possible)
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 50.0, Strength.Weak));

        solver.Solve();

        // x should be 50 (weak preference satisfied since it doesn't violate x >= 10)
        Assert.AreEqual(50.0, x.Value, Epsilon);

        // Now add a stronger constraint pulling x towards 5, but x >= 10 must hold
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 5.0, Strength.Strong));
        solver.Solve();

        // x should be 10 (x >= 10 is required, strong constraint wants 5 but can't go below 10)
        Assert.That(x.Value, Is.GreaterThanOrEqualTo(10.0 - Epsilon),
            "x must satisfy required constraint x >= 10");
    }

    [Test]
    public void DualOptimize_EditVariableWithBounds()
    {
        // Classic use case: edit variable with inequality bounds
        Solver solver = new();
        Variable x = new("x");

        // Bounds: 0 <= x <= 100
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.GreaterThanOrEqual, 0.0));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.LessThanOrEqual, 100.0));

        solver.AddEditVariable(x, Strength.Strong);

        // Suggest value within bounds
        solver.SuggestValue(x, 50.0);
        solver.Solve();
        Assert.AreEqual(50.0, x.Value, Epsilon, "x should be 50 (within bounds)");

        // Suggest value beyond upper bound — should clamp to 100
        solver.SuggestValue(x, 150.0);
        solver.Solve();
        Assert.AreEqual(100.0, x.Value, Epsilon, "x should be clamped to 100");

        // Suggest value below lower bound — should clamp to 0
        solver.SuggestValue(x, -50.0);
        solver.Solve();
        Assert.AreEqual(0.0, x.Value, Epsilon, "x should be clamped to 0");
    }

    // =========================================================================
    // Redundant required constraints should not throw
    // =========================================================================

    [Test]
    public void RedundantRequiredConstraint_ShouldNotThrow()
    {
        // When all terms are dummies (required equality) and the constant is zero/near-zero,
        // the constraint is redundant but satisfiable. Should NOT throw.
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        // x + y == 10, x == 6, y == 4 → all consistent
        // The third constraint is algebraically redundant once the first two are added.
        // After substitution, the row will be all dummies with constant ≈ 0.
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 6.0));
        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 4.0));

        Assert.DoesNotThrow(() =>
        {
            solver.AddConstraint(Constraint.Make(x + y, RelationalOperator.Equal, 10.0));
        }, "Redundant but satisfiable required constraint should not throw");

        solver.Solve();
        Assert.AreEqual(6.0, x.Value, Epsilon);
        Assert.AreEqual(4.0, y.Value, Epsilon);
    }

    [Test]
    public void UnsatisfiableConstraint_ShouldThrow()
    {
        // When all terms are dummies and the constant is NOT near-zero,
        // the constraint is genuinely unsatisfiable. Should throw.
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        // x == 6, y == 4, x + y == 20 → inconsistent (6 + 4 ≠ 20)
        // After substitution, the row will be all dummies with constant ≈ 10 (non-zero).
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 6.0));
        solver.AddConstraint(Constraint.Make(y, RelationalOperator.Equal, 4.0));

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Make(x + y, RelationalOperator.Equal, 20.0));
        }, "Conflicting required constraints should throw UnsatisfiableConstraintException");
    }


    // =========================================================================
    // Strength ordering should be lexicographic
    // =========================================================================

    [Test]
    public void StrengthOrdering_ShouldBeLexicographic()
    {
        Solver solver = new();
        Variable x = new("x");

        // Strong should dominate any combination of mediums.
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 0.0, Strength.Strong));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 10.0, Strength.Medium));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.Equal, 20.0, Strength.Medium));

        solver.Solve();

        Assert.AreEqual(0.0, x.Value, Epsilon,
            "Lexicographic strengths should keep x near the strong constraint, not the combined mediums");
    }

    // =========================================================================
    // WithTransaction should re-optimize when AutoSolve is enabled
    // =========================================================================

    [Test]
    public void WithTransaction_ShouldDualOptimizeWhenAutoSolveEnabled()
    {
        Solver solver = new();
        Variable x = new("x");

        solver.AutoSolve = true;
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.GreaterThanOrEqual, 0.0));
        solver.AddConstraint(Constraint.Make(x, RelationalOperator.LessThanOrEqual, 100.0));
        solver.AddEditVariable(x, Strength.Strong);

        solver.WithTransaction(s =>
        {
            s.SuggestValue(x, 150.0);
        });

        Assert.AreEqual(100.0, x.Value, Epsilon,
            "AutoSolve should re-optimize after a transaction and clamp to bounds");
    }

    // =========================================================================
    // Expression hash code should match Expression equality
    // =========================================================================

    [Test]
    public void ExpressionHashCode_ShouldRespectEquality()
    {
        Variable x = new("x");

        Expression expr1 = new(new[] { new Term(x), new Term(x) });
        Expression expr2 = new(new[] { new Term(x, 2.0) });

        Assert.AreEqual(expr1, expr2, "Expressions with the same coefficients should be equal");

        var dict = new System.Collections.Generic.Dictionary<Expression, string>();
        dict[expr1] = "value";

        Assert.IsTrue(dict.ContainsKey(expr2),
            "Hash code should be consistent with equality so dictionaries can find equal expressions");
    }

}
