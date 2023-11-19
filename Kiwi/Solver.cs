using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Nanoray.Kiwi;

/// <summary>Describes a linear equation/inequality constraint solver system.</summary>
public sealed class Solver
{
    internal record struct Tag(
        Symbol Marker,
        Symbol? Other
    );

    private sealed class EditInfo
    {
        internal Tag Tag { get; init; }
        internal Constraint Constraint { get; init; }
        internal double Constant { get; set; }

        internal EditInfo(Tag tag, Constraint constraint, double constant)
        {
            this.Tag = tag;
            this.Constraint = constraint;
            this.Constant = constant;
        }
    };

    private sealed class VariableInfo
    {
        internal Variable Variable { get; init; }
        internal Symbol Symbol { get; init; }
        internal EditInfo? Edit { get; set; }
        internal int ReferenceCount { get; set; }

        internal VariableInfo(Variable variable, Symbol symbol, EditInfo? edit = null, int referenceCount = 0)
        {
            this.Variable = variable;
            this.Symbol = symbol;
            this.Edit = edit;
            this.ReferenceCount = referenceCount;
        }
    }

    /// <summary>Whether the solver should automatically re-solve the equation system when adding new constraints.</summary>
    public bool AutoSolve
    {
        get => _AutoSolve;
        set
        {
            if (!_AutoSolve && value)
                DualOptimize();
            _AutoSolve = value;
        }
    }

    private int NextSymbolID { get; set; } = 0;
    private Dictionary<Constraint, Tag> Constraints { get; set; } = new();
    private OrderedDictionary<Symbol, Row> Rows { get; set; } = new();
    private Dictionary<Variable, VariableInfo> Variables { get; set; } = new();
    private List<Symbol> InfeasibleRows { get; set; } = new();
    private Row Objective { get; set; } = new();
    private Row? Artificial { get; set; }

    private bool _AutoSolve = false;

    /// <summary>Starts a new solver system transaction.</summary>
    /// <remarks>The solver will not try to re-solve the equation system until the end of the provided closure.</remarks>
    /// <param name="closure">The actions to execute on the solver, before trying to re-solve the equation system.</param>
    public void WithTransaction(Action<Solver> closure)
    {
        bool oldAutoSolve = _AutoSolve;
        _AutoSolve = false;

        closure(this);

        if (oldAutoSolve)
        {
            _AutoSolve = true;
            UpdateVariables();
        }
        else
        {
            Solve();
        }
    }

    /// <summary>Solves the equation system.</summary>
    public void Solve()
    {
        DualOptimize();
        UpdateVariables();
    }

    /// <summary>Updates the variables' values according to the current state of the (solved) equation system.</summary>
    /// <remarks>This method should only be used when working with a <see cref="Solver"/> that has <see cref="AutoSolve"/> set to <c>true</c>; otherwise it is automatically called when appropriate.</remarks>
    public void UpdateVariables()
    {
        FlushUnusedVariables();
        foreach (var variableInfo in this.Variables.Values)
            variableInfo.Variable.Store.Value = this.Rows.TryGetValue(variableInfo.Symbol, out var row) ? row.Constant : 0;
    }

    private void FlushUnusedVariables()
    {
        foreach (var info in Variables.Values)
            if (info.ReferenceCount <= 0)
                Variables.Remove(info.Variable);
    }

    /// <summary>Add a constraint to the solver system.</summary>
    /// <param name="constraint">The constraint to add.</param>
    /// <exception cref="DuplicateConstraintException">Thrown if the constraint is already added to the solver system.</exception>
    public void AddConstraint(Constraint constraint)
    {
        if (!TryAddConstraint(constraint))
            throw new DuplicateConstraintException(constraint);
    }

    /// <summary>Try to add a constraint to the solver system.</summary>
    /// <param name="constraint">The constraint to add</param>
    /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise (if the constraint is already added to the solver system).</returns>
    public bool TryAddConstraint(Constraint constraint)
        => PrivateTryAddConstraint(constraint) != null;

    private Tag? PrivateTryAddConstraint(Constraint constraint)
    {
        if (Constraints.ContainsKey(constraint))
            return null;

        CreateRow(constraint, out var row, out var tag);

        if (GetSubject(constraint, row, ref tag) is { } subject)
        {
            row.SolveForSymbol(subject);
            Substitute(subject, row);
            this.Rows[subject] = row;
        }

        this.Constraints[constraint] = tag;
        Optimize(this.Objective);
        return tag;
    }

    /// <summary>Remove a constraint from the solver system.</summary>
    /// <param name="constraint">The constraint to remove.</param>
    /// <exception cref="UnknownConstraintException">Thrown if the constraint is not added to the solver system.</exception>
    public void RemoveConstraint(Constraint constraint)
    {
        if (!TryRemoveConstraint(constraint))
            throw new UnknownConstraintException(constraint);
    }

    /// <summary>Try to remove a constraint from the solver system.</summary>
    /// <param name="constraint">The constraint to remove.</param>
    /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise (if the constraint is not added to the solver system).</returns>
    public bool TryRemoveConstraint(Constraint constraint)
    {
        if (!this.Constraints.TryGetValue(constraint, out var tag))
            return false;

        foreach (var term in constraint.Expression._Terms)
        {
            if (!Util.IsNearZero(term.Coefficient))
                continue;
            if (!this.Variables.TryGetValue(term.Variable, out var info))
                continue;
            info.ReferenceCount--;
        }

        RemoveConstraintEffects(constraint, tag);

        if (!this.Rows.Remove(tag.Marker))
        {
            var (leaving, row) = GetMarkerLeavingRow(tag.Marker) ?? throw new InternalSolverException();
            this.Rows.Remove(leaving);
            row.SolveForSymbols(leaving, tag.Marker);
            Substitute(tag.Marker, row);
        }

        Optimize(this.Objective);
        return true;
    }

    /// <summary>Check whether the constraint is added to the solver system.</summary>
    /// <param name="constraint">The constraint to check.</param>
    /// <returns><c>true</c> if the constraint is added to the solver system, <c>false</c> otherwise.</returns>
    public bool HasConstraint(Constraint constraint)
        => this.Constraints.ContainsKey(constraint);

    /// <summary>Add an edit constraint on the provided variable, allowing the use of <see cref="SuggestValue(Variable, double)"/>.</summary>
    /// <param name="variable">The variable to add the edit constraint on.</param>
    /// <param name="strength">The strength of the constraint to add. The strength cannot be <see cref="Strength.Required"/>.</param>
    /// <exception cref="DuplicateEditVariableException">Thrown if the variable already has an edit constraint attached.</exception>
    public void AddEditVariable(Variable variable, double strength)
    {
        if (!TryAddEditVariable(variable, strength))
            throw new DuplicateEditVariableException();
    }

    /// <summary>Try to add an edit constraint on the provided variable, allowing the use of <see cref="SuggestValue(Variable, double)"/>.</summary>
    /// <param name="variable">The variable to add the edit constraint on.</param>
    /// <param name="strength">The strength of the constraint to add. The strength cannot be <see cref="Strength.Required"/>.</param>
    /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise (if the variable already has an edit constraint attached).</returns>
    public bool TryAddEditVariable(Variable variable, double strength)
    {
        var variableInfo = ObtainInfo(variable);
        if (variableInfo.Edit is not null)
            return false;

        strength = Strength.Clip(strength);
        if (strength == Strength.Required)
            throw new ArgumentException("Strength cannot be Required");

        Term term = new(variable);
        Constraint constraint = new(new Expression(term), RelationalOperator.Equal, strength);

        if (PrivateTryAddConstraint(constraint) is { } tag)
            variableInfo.Edit = new(tag, constraint, 0);
        return true;
    }

    /// <summary>Remove an edit constraint from the provided variable.</summary>
    /// <param name="variable">The variable to remove the edit constraint from.</param>
    /// <exception cref="UnknownEditVariableException">Thrown if the variable does not have an edit constraint attached.</exception>
    /// <seealso cref="AddEditVariable(Variable, double)"/>
    public void RemoveEditVariable(Variable variable)
    {
        if (!TryRemoveEditVariable(variable))
            throw new UnknownEditVariableException();
    }

    /// <summary>Try to remove an edit constraint from the provided variable.</summary>
    /// <param name="variable">The variable to remove the edit constraint from.</param>
    /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise (if the variable does not have an edit constraint attached).</returns>
    /// <seealso cref="TryAddEditVariable(Variable, double)"/>
    public bool TryRemoveEditVariable(Variable variable)
    {
        var variableInfo = GetInfo(variable);
        if (variableInfo?.Edit is null)
            return false;

        TryRemoveConstraint(variableInfo.Edit.Constraint);
        variableInfo.Edit = null;
        return true;
    }

    /// <summary>Check whether the variable has an edit constraint attached.</summary>
    /// <param name="variable">The variable to check.</param>
    /// <returns><c>true</c> if the variable has an edit constraint attached, <c>false</c> otherwise.</returns>
    /// <seealso cref="AddEditVariable(Variable, double)"/>
    public bool HasEditVariable(Variable variable)
        => GetInfo(variable)?.Edit is not null;

    /// <summary>Specify a desired value for the provided variable.</summary>
    /// <remarks>The variable needs to have been previously attached with an edit constraint.</remarks>
    /// <param name="variable">The variable.</param>
    /// <param name="value">The value for the variable.</param>
    /// <exception cref="UnknownEditVariableException">Thrown if the variable does not have an edit constraint attached.</exception>
    /// <seealso cref="AddEditVariable(Variable, double)"/>
    public void SuggestValue(Variable variable, double value)
    {
        var variableInfo = GetInfo(variable);
        if (variableInfo?.Edit is null)
            throw new UnknownEditVariableException();

        double delta = value - variableInfo.Edit.Constant;
        variableInfo.Edit.Constant = value;

        {
            // Check first if the positive error variable is basic.
            if (this.Rows.TryGetValue(variableInfo.Edit.Tag.Marker, out var row))
            {
                if (row.Add(-delta) < 0)
                    this.InfeasibleRows.Add(variableInfo.Edit.Tag.Marker);
                goto Finish;
            }

            // Check next if the negative error variable is basic.
            if (variableInfo.Edit.Tag.Other is { } other && this.Rows.TryGetValue(other, out row))
            {
                if (row.Add(-delta) < 0)
                    this.InfeasibleRows.Add(other);
                goto Finish;
            }
        }

        // Otherwise update each row where the error variables exist.
        foreach (var (symbol, row) in this.Rows)
        {
            double coefficient = row.GetCoefficientForSymbol(variableInfo.Edit.Tag.Marker);
            if (coefficient != 0 && row.Add(delta * coefficient) < 0 && symbol.Type != SymbolType.External)
                this.InfeasibleRows.Add(symbol);
        }

        Finish:
        if (this._AutoSolve)
            DualOptimize();
    }

    private Symbol? GetSubject(Constraint constraint, Row row, ref Tag tag)
    {
        var subject = ChooseSubject(row, ref tag);
        if (subject is not null)
            return subject;

        if (row.AreAllDummies())
        {
            if (Util.IsNearZero(row.Constant))
                throw new UnsatisfiableConstraintException(constraint);
            else
                return tag.Marker;
        }

        if (!AddWithArtificialVariable(row))
            throw new UnsatisfiableConstraintException(constraint);
        return null;
    }

    private void RemoveConstraintEffects(Constraint constraint, Tag tag)
    {
        if (tag.Marker.Type == SymbolType.Error)
            RemoveMarkerEffects(tag.Marker, constraint.Strength);
        else if (tag.Other is { } other && other.Type == SymbolType.Error)
            RemoveMarkerEffects(other, constraint.Strength);
    }

    private void RemoveMarkerEffects(Symbol marker, double strength)
    {
        if (this.Rows.TryGetValue(marker, out var row))
            this.Objective.Insert(row, -strength);
        else
            this.Objective.Insert(marker, -strength);
    }

    private (Symbol, Row)? GetMarkerLeavingRow(Symbol marker)
    {
        double ratio1 = double.MaxValue;
        double ratio2 = double.MaxValue;

        (Symbol, Row)? first = null;
        (Symbol, Row)? second = null;
        (Symbol, Row)? third = null;

        foreach (var (symbol, row) in this.Rows)
        {
            double coefficient = row.GetCoefficientForSymbol(marker);
            if (coefficient == 0)
                continue;

            if (symbol.Type == SymbolType.External)
            {
                third = (symbol, row);
            }
            else if (coefficient < 0)
            {
                double newRatio = -row.Constant / coefficient;
                if (newRatio < ratio1)
                {
                    ratio1 = newRatio;
                    first = (symbol, row);
                }
            }
            else
            {
                double newRatio = row.Constant / coefficient;
                if (newRatio < ratio2)
                {
                    ratio2 = newRatio;
                    second = (symbol, row);
                }
            }
        }

        return first ?? second ?? third;
    }

    /// <summary>Create a new Row object for the given constraint.</summary>
    /// <remarks>
    /// <para>
    /// The terms in the constraint will be converted to cells in the row.
    /// Any term in the constraint with a coefficient of zero is ignored.
    /// This method uses the `getVarSymbol` method to get the symbol for
    /// the variables added to the row.If the symbol for a given cell
    /// variable is basic, the cell variable will be substituted with the
    /// basic row.
    /// </para>
    /// <para>
    /// The necessary slack and error variables will be added to the row.
    /// If the constant for the row is negative, the sign for the row
    /// will be inverted so the constant becomes positive.
    /// </para>
    /// <para>
    /// The tag will be updated with the marker and error symbols to use
    /// for tracking the movement of the constraint in the tableau.
    /// </para>
    /// </remarks>
    private void CreateRow(Constraint constraint, out Row row, out Tag tag)
    {
        row = new(constraint.Expression.Constant);
        Symbol marker;
        Symbol? other = null;

        foreach (var term in constraint.Expression._Terms)
        {
            if (Util.IsNearZero(term.Coefficient))
                continue;

            var info = ObtainInfo(term.Variable);
            info.ReferenceCount += 1;

            if (this.Rows.TryGetValue(info.Symbol, out var otherRow))
                row.Insert(otherRow, term.Coefficient);
            else
                row.Insert(info.Symbol, term.Coefficient);
        }

        switch (constraint.Operator)
        {
            case RelationalOperator.LessThanOrEqual:
            case RelationalOperator.GreaterThanOrEqual:
                double coefficient = constraint.Operator == RelationalOperator.LessThanOrEqual ? 1 : -1;
                Symbol slack = CreateSymbol(SymbolType.Slack);
                marker = slack;
                row.Insert(slack, coefficient);

                if (constraint.Strength < Strength.Required)
                {
                    Symbol error = CreateSymbol(SymbolType.Error);
                    other = error;
                    row.Insert(error, -coefficient);
                    this.Objective.Insert(error, constraint.Strength);
                }
                break;
            case RelationalOperator.Equal:
                if (constraint.Strength < Strength.Required)
                {
                    Symbol errorPlus = CreateSymbol(SymbolType.Error);
                    Symbol errorMinus = CreateSymbol(SymbolType.Error);
                    marker = errorPlus;
                    other = errorMinus;
                    row.Insert(errorPlus, -1); // v = ePlus - eMinus
                    row.Insert(errorMinus, 1); // v - ePlus + eMinus = 0
                    this.Objective.Insert(errorPlus, constraint.Strength);
                    this.Objective.Insert(errorMinus, constraint.Strength);
                }
                else
                {
                    Symbol dummy = CreateSymbol(SymbolType.Dummy);
                    marker = dummy;
                    row.Insert(dummy);
                }
                break;
            default:
                throw new ArgumentException($"Unhandled `RelationalOperator` {constraint.Operator}");
        }

        // Ensure the row as a positive constant.
        if (row.Constant < 0)
            row.ReverseSign();

        tag = new(marker, other);
    }

    /// <summary>Choose the subject for solving for the row.</summary>
    /// <remarks>
    /// <para>
    /// This method will choose the best subject for using as the solve
    /// target for the row.An invalid symbol will be returned if there
    /// is no valid target.
    /// </para>
    /// <para>
    /// The symbols are chosen according to the following precedence:
    /// <list type="number">
    /// <item>The first symbol representing an external variable.</item>
    /// <item>A negative slack or error tag variable.</item>
    /// </list>
    /// If a subject cannot be found, an invalid symbol will be returned.
    /// </para>
    /// </remarks>
    private static Symbol? ChooseSubject(Row row, ref Tag tag)
    {
        foreach (var symbol in row.Cells.Keys)
            if (symbol.Type == SymbolType.External)
                return symbol;
        if (tag.Marker.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(tag.Marker) < 0)
                return tag.Marker;
        if (tag.Other is { } other && other.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(other) < 0)
                return other;
        return null;
    }

    /// <summary>Add the row to the tableau using an artificial variable.</summary>
    /// <remarks>This will return false if the constraint cannot be satisfied.</remarks>
    private bool AddWithArtificialVariable(Row row)
    {
        // Create and add the artificial variable to the tableau
        Symbol artificial = CreateSymbol(SymbolType.Slack);
        this.Rows[artificial] = new(row);
        this.Artificial = new(row);

        // Optimize the artificial objective. This is successful
        // only if the artificial objective is optimized to zero.
        Optimize(this.Artificial);

        bool success = Util.IsNearZero(this.Artificial.Constant);
        this.Artificial = null;

        // If the artificial variable is basic, pivot the row so that
        // it becomes basic. If the row is constant, exit early.

        if (this.Rows.TryGetValue(artificial, out var rowPointer))
        {
            this.Rows.Remove(artificial);
            if (rowPointer.Cells.Count == 0)
                return success;

            if (rowPointer.GetAnyPivotableSymbol() is not { } entering)
                return false; // unsatisfiable (will this ever happen?)

            rowPointer.SolveForSymbols(artificial, entering);
            Substitute(entering, rowPointer);
            this.Rows[entering] = rowPointer;
        }

        // Remove the artificial variable from the tableau.
        foreach (var existingRow in this.Rows.Values)
            existingRow.Remove(artificial);

        this.Objective.Remove(artificial);
        return success;
    }

    /// <summary>Substitute the parametric symbol with the given row.</summary>
    /// <remarks>
    /// This method will substitute all instances of the parametric symbol
    /// in the tableau and the objective function with the given row.
    /// </remarks>
    private void Substitute(Symbol symbol, Row row)
    {
        foreach (var (existingSymbol, existingRow) in this.Rows)
        {
            existingRow.Substitute(symbol, row);
            if (existingSymbol.Type != SymbolType.External && existingRow.Constant < 0)
                this.InfeasibleRows.Add(existingSymbol);
        }

        this.Objective.Substitute(symbol, row);
        this.Artificial?.Substitute(symbol, row);
    }

    /// <summary>Optimize the system for the given objective function.</summary>
    /// <remarks>
    /// This method performs iterations of Phase 2 of the simplex method
    /// until the objective function reaches a minimum.
    /// </remarks>
    private void Optimize(Row objective)
    {
        while (true)
        {
            if (objective.GetEnteringSymbol() is not { } entering)
                return;
            var (leaving, entry) = GetLeavingRow(entering) ?? throw new InternalSolverException("The objective is unbounded");

            this.Rows.Remove(leaving);
            entry.SolveForSymbols(leaving, entering);
            Substitute(entering, entry);
            this.Rows[entering] = entry;
        }
    }

    private void DualOptimize()
    {
        while (this.InfeasibleRows.Count != 0)
        {
            var leaving = this.InfeasibleRows[^1];
            this.InfeasibleRows.RemoveAt(this.InfeasibleRows.Count - 1);
            if (!this.Rows.TryGetValue(leaving, out var row) || row.Constant < 0)
                continue;

            var entering = GetDualEnteringSymbol(row) ?? throw new InternalSolverException();
            this.Rows.Remove(leaving);
            row.SolveForSymbols(leaving, entering);
            Substitute(entering, row);
            this.Rows[entering] = row;
        }
    }

    private Symbol? GetDualEnteringSymbol(Row row)
    {
        Symbol? entering = null;
        double ratio = double.MaxValue;

        foreach (var (symbol, value) in row.Cells)
        {
            if (symbol.Type == SymbolType.Dummy || value <= 0)
                continue;

            double coefficient = this.Objective.GetCoefficientForSymbol(symbol);
            double newRatio = coefficient / value;
            if (newRatio < ratio)
            {
                ratio = newRatio;
                entering = symbol;
            }
        }

        return entering;
    }

    /// <summary>Compute the row which holds the exit symbol for a pivot.</summary>
    /// <remarks>
    /// <para>This documentation is copied from the C++ version and is outdated.</para>
    /// <para>
    /// This method will return an iterator to the row in the row map
    /// which holds the exit symbol. If no appropriate exit symbol is
    /// found, `nil` will be returned. This indicates that
    /// the objective function is unbounded.
    /// </para>
    /// </remarks>
    private (Symbol, Row)? GetLeavingRow(Symbol entering)
    {
        double ratio = double.MaxValue;
        (Symbol, Row)? leavingRow = null;

        foreach (var (symbol, candidateRow) in this.Rows)
        {
            if (symbol.Type == SymbolType.External)
                continue;

            double coefficient = candidateRow.GetCoefficientForSymbol(entering);
            if (coefficient >= 0)
                continue;

            double newRatio = -candidateRow.Constant / coefficient;
            if (newRatio < ratio)
            {
                ratio = newRatio;
                leavingRow = (symbol, candidateRow);
            }
        }

        return leavingRow;
    }

    private Symbol CreateSymbol(SymbolType type)
        => new(++this.NextSymbolID, type);

    private VariableInfo? GetInfo(Variable variable)
        => this.Variables.GetValueOrNull(variable);

    private VariableInfo ObtainInfo(Variable variable)
    {
        if (!this.Variables.TryGetValue(variable, out var info))
        {
            info = new(variable, CreateSymbol(SymbolType.External));
            this.Variables[variable] = info;
        }
        return info;
    }
}
