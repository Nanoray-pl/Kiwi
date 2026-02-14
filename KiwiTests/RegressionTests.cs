using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

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
        var result = 2.0 * expr;

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
            s.AddConstraint(Constraint.Equal(x, 10.0));
            // 2.0 * x uses: double * Variable → double * Term → double * Expression
            s.AddConstraint(Constraint.Equal(y, 2.0 * x));
        });
        solver.Solve();

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
        solver.AddConstraint(Constraint.GreaterEqual(x, 10.0));

        // x == 50 (weak — should be satisfied when possible)
        solver.AddConstraint(Constraint.Equal(x, 50.0, Strength.Weak));

        solver.Solve();

        // x should be 50 (weak preference satisfied since it doesn't violate x >= 10)
        Assert.AreEqual(50.0, x.Value, Epsilon);

        // Now add a stronger constraint pulling x towards 5, but x >= 10 must hold
        solver.AddConstraint(Constraint.Equal(x, 5.0, Strength.Strong));
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
        solver.AddConstraint(Constraint.GreaterEqual(x, 0.0));
        solver.AddConstraint(Constraint.LessEqual(x, 100.0));

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
        solver.AddConstraint(Constraint.Equal(x, 6.0));
        solver.AddConstraint(Constraint.Equal(y, 4.0));

        Assert.DoesNotThrow(() =>
        {
            solver.AddConstraint(Constraint.Equal(x + y, 10.0));
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
        solver.AddConstraint(Constraint.Equal(x, 6.0));
        solver.AddConstraint(Constraint.Equal(y, 4.0));

        Assert.Throws<UnsatisfiableConstraintException>(() =>
        {
            solver.AddConstraint(Constraint.Equal(x + y, 20.0));
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
        solver.AddConstraint(Constraint.Equal(x, 0.0, Strength.Strong));
        solver.AddConstraint(Constraint.Equal(x, 10.0, Strength.Medium));
        solver.AddConstraint(Constraint.Equal(x, 20.0, Strength.Medium));

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
        solver.AddConstraint(Constraint.GreaterEqual(x, 0.0));
        solver.AddConstraint(Constraint.LessEqual(x, 100.0));
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

        var dict = new Dictionary<Expression, string>
        {
            [expr1] = "value"
        };

        Assert.IsTrue(dict.ContainsKey(expr2),
            "Hash code should be consistent with equality so dictionaries can find equal expressions");
    }

    // =========================================================================
    // Correctness: WithTransaction should restore AutoSolve on exception
    // =========================================================================

    [Test]
    public void WithTransaction_ExceptionShouldRestoreAutoSolve()
    {
        Solver solver = new();
        Variable x = new("x");
        solver.AutoSolve = true;

        try
        {
            solver.WithTransaction(s =>
            {
                s.AddConstraint(Constraint.Equal(x, 10.0));
                throw new InvalidOperationException("boom");
            });
        }
        catch (InvalidOperationException)
        {
            // swallow
        }

        Assert.IsTrue(solver.AutoSolve,
            "AutoSolve should be restored even if the transaction throws");
    }

    // =========================================================================
    // Correctness: TryAddEditVariable should not fail for value-equal constraints
    // =========================================================================

    [Test]
    public void TryAddEditVariable_ShouldSucceedWhenValueEqualConstraintExists()
    {
        Solver solver = new();
        Variable x = new("x");

        Constraint existing = new(new Expression(new Term(x)), RelationalOperator.Equal, Strength.Weak);
        solver.AddConstraint(existing);

        bool added = solver.TryAddEditVariable(x, Strength.Weak);

        Assert.IsTrue(added,
            "TryAddEditVariable should succeed when only a value-equal constraint exists");
        Assert.IsTrue(solver.HasEditVariable(x),
            "Edit variable should be attached when the add succeeds");
    }

    // =========================================================================
    // Correctness: Expression.Terms should not be mutable from the public API
    // =========================================================================

    [Test]
    public void ExpressionTerms_ShouldBeImmutable()
    {
        Solver solver = new();
        Variable x = new("x");

        Constraint c = Constraint.Equal(new Expression(new Term(x)), 0.0);
        solver.AddConstraint(c);

        var terms = c.Expression.Terms;

        Assert.That(terms, Is.Not.InstanceOf<Term[]>());
        Assert.Throws<NotSupportedException>(() => ((IList<Term>)terms)[0] = new Term(x, 2.0));
        Assert.IsTrue(solver.HasConstraint(c),
            "Constraint lookup should remain valid when terms are immutable");
    }

    // =========================================================================
    // Constraint identity should be handle-based, not value-based
    // =========================================================================

    [Test]
    public void ConstraintIdentity_ValueEqualIsNotDuplicate()
    {
        Solver solver = new();
        Variable x = new("x");

        Constraint c1 = Constraint.Equal(x, 10.0);
        Constraint c2 = Constraint.Equal(x, 10.0);

        solver.AddConstraint(c1);

        Assert.DoesNotThrow(() => solver.AddConstraint(c2),
            "Value-equal constraints should be treated as distinct handles");
        Assert.IsTrue(solver.HasConstraint(c1));
        Assert.IsTrue(solver.HasConstraint(c2));
    }

    [Test]
    public void ConstraintIdentity_ValueEqualRemoveShouldThrow()
    {
        Solver solver = new();
        Variable x = new("x");

        Constraint c1 = Constraint.Equal(x, 10.0);
        Constraint c2 = Constraint.Equal(x, 10.0);

        solver.AddConstraint(c1);

        Assert.Throws<UnknownConstraintException>(() => solver.RemoveConstraint(c2),
            "Removing a value-equal constraint should not affect the stored handle");
        Assert.IsTrue(solver.HasConstraint(c1));
    }

    [Test]
    public void ConstraintIdentity_ValueEqualHasConstraintShouldBeFalse()
    {
        Solver solver = new();
        Variable x = new("x");

        Constraint c1 = Constraint.Equal(x, 10.0);
        Constraint c2 = Constraint.Equal(x, 10.0);

        solver.AddConstraint(c1);

        Assert.IsFalse(solver.HasConstraint(c2),
            "Handle identity should not treat value-equal constraints as the same instance");
    }

    // =========================================================================
    // Variable identity should be reference-based, not value-based
    // =========================================================================

    [Test]
    public void VariableIdentity_SameNameShouldBeIndependent()
    {
        Solver solver = new();
        Variable x1 = new("x");
        Variable x2 = new("x");

        solver.AddConstraint(Constraint.Equal(x1, 10.0));
        solver.AddConstraint(Constraint.Equal(x2, 20.0));
        solver.Solve();

        Assert.AreEqual(10.0, x1.Value, Epsilon, "x1 should be 10");
        Assert.AreEqual(20.0, x2.Value, Epsilon,
            "x2 should be 20 — same-name variables must be independent instances");
    }

    // =========================================================================
    // DualOptimize should skip near-zero infeasible rows (matches C++ behavior)
    // =========================================================================

    [Test]
    public void DualOptimize_ShouldSkipNearZeroInfeasibleRows()
    {
        // The C++ reference skips rows whose constant is negative but near-zero,
        // treating them as non-infeasible. Without this check, the solver may
        // attempt unnecessary (or harmful) pivots on rows that are only
        // infeasible due to floating-point rounding.
        Solver solver = new();
        Variable x = new("x");
        Variable y = new("y");

        // Build a system that, after solving, leaves rows with tiny negative
        // constants due to floating-point arithmetic.
        solver.AddConstraint(Constraint.GreaterEqual(x, 0.0));
        solver.AddConstraint(Constraint.GreaterEqual(y, 0.0));
        solver.AddConstraint(Constraint.Equal(x + y, 100.0));

        solver.AddEditVariable(x, Strength.Strong);
        solver.AddEditVariable(y, Strength.Strong);

        // Suggest values that exactly satisfy the equality — the residual
        // should be zero, but floating-point may leave a tiny negative.
        solver.SuggestValue(x, 50.0);
        solver.SuggestValue(y, 50.0);

        // This should not throw InternalSolverException from a failed
        // dual pivot on a near-zero row.
        Assert.DoesNotThrow(() => solver.Solve(),
            "DualOptimize should skip near-zero infeasible rows instead of pivoting");

        Assert.AreEqual(50.0, x.Value, Epsilon);
        Assert.AreEqual(50.0, y.Value, Epsilon);
    }

}
