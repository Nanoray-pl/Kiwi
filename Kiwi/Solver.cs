using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public sealed class Solver
{
    private record struct Tag(
        Symbol Marker,
        Symbol? Other
    );

    private sealed class EditInfo
    {
        public Tag Tag { get; init; }
        public Constraint Constraint { get; init; }
        public double Constant { get; set; }

        public EditInfo(Tag tag, Constraint constraint, double constant)
        {
            this.Tag = tag;
            this.Constraint = constraint;
            this.Constant = constant;
        }
    };

    private sealed class VariableInfo
    {
        public IVariable Variable { get; init; }
        public Symbol Symbol { get; init; }
        public EditInfo? Edit { get; set; }

        public VariableInfo(IVariable variable, Symbol symbol, EditInfo? edit = null)
        {
            this.Variable = variable;
            this.Symbol = symbol;
            this.Edit = edit;
        }
    }

    private int NextSymbolID { get; set; } = 0;
    private Dictionary<Constraint, Tag> Constraints { get; set; } = new();
    private OrderedDictionary<Symbol, Row> Rows { get; set; } = new();
    private Dictionary<IVariable, VariableInfo> Variables { get; set; } = new();
    private List<Symbol> InfeasibleRows { get; set; } = new();
    private Row Objective { get; set; } = new();
    private Row? Artificial { get; set; }

    public void AddConstraint(Constraint constraint)
    {
        if (Constraints.ContainsKey(constraint))
            throw new DuplicateConstraintException(constraint);

        var (row, tag) = CreateRow(constraint);
        var subject = ChooseSubject(row, tag);

        if (subject is null && AreAllDummies(row))
        {
            if (Util.IsNearZero(row.Constant))
                throw new UnsatisfiableConstraintException(constraint);
            else
                subject = tag.Marker;
        }

        if (subject is null)
        {
            if (!AddWithArtificialVariable(row))
                throw new UnsatisfiableConstraintException(constraint);
        }
        else
        {
            row.SolveForSymbol(subject.Value);
            Substitute(subject.Value, row);
            this.Rows[subject.Value] = row;
        }

        this.Constraints[constraint] = tag;
        Optimize(this.Objective);
    }

    public bool TryRemoveConstraint(Constraint constraint)
    {
        if (!this.Constraints.TryGetValue(constraint, out var tag))
            return false;

        this.Constraints.Remove(constraint);
        RemoveConstraintEffects(constraint, tag);

        if (!this.Rows.Remove(tag.Marker))
        {
            var row = GetMarkerLeavingRow(tag.Marker) ?? throw new InternalSolverException();
            var leaving = this.Rows.FirstOrNull(kvp => kvp.Value == row)?.Key ?? throw new InternalSolverException();

            this.Rows.Remove(leaving);
            row.SolveForSymbols(leaving, tag.Marker);
            Substitute(tag.Marker, row);
        }

        Optimize(this.Objective);
        return true;
    }

    public void RemoveConstraint(Constraint constraint)
    {
        if (!TryRemoveConstraint(constraint))
            throw new UnknownConstraintException(constraint);
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

    private Row? GetMarkerLeavingRow(Symbol marker)
    {
        double ratio1 = double.MaxValue;
        double ratio2 = double.MaxValue;

        Row? first = null;
        Row? second = null;
        Row? third = null;

        foreach (var (symbol, row) in this.Rows)
        {
            double coefficient = row.GetCoefficientForSymbol(marker);
            if (coefficient == 0)
                continue;

            if (symbol.Type == SymbolType.External)
            {
                third = row;
            }
            else if (coefficient < 0)
            {
                double newRatio = -row.Constant / coefficient;
                if (newRatio < ratio1)
                {
                    ratio1 = newRatio;
                    first = row;
                }
            }
            else
            {
                double newRatio = row.Constant / coefficient;
                if (newRatio < ratio2)
                {
                    ratio2 = newRatio;
                    second = row;
                }
            }
        }

        return first ?? second ?? third;
    }

    public bool HasConstraint(Constraint constraint)
        => this.Constraints.ContainsKey(constraint);

    public void AddEditVariable(IVariable variable, double strength)
    {
        var variableInfo = ObtainInfo(variable);
        if (variableInfo.Edit is not null)
            throw new DuplicateEditVariableException();

        strength = Strength.Clip(strength);
        if (strength == Strength.Required)
            throw new ArgumentException("Strength cannot be Required");

        Term term = new(variable);
        Constraint constraint = new(new Expression(term), RelationalOperator.Equal, strength);

        AddConstraint(constraint); // TODO: try catch and ignore, or rework that thing to not work on exceptions
        if (!this.Constraints.TryGetValue(constraint, out var tag))
            throw new InternalSolverException();
        variableInfo.Edit = new(tag, constraint, 0);
    }

    public bool TryRemoveEditVariable(IVariable variable)
    {
        var variableInfo = GetInfo(variable);
        if (variableInfo?.Edit is null)
            return false;

        TryRemoveConstraint(variableInfo.Edit.Constraint);
        variableInfo.Edit = null;
        return true;
    }

    public void RemoveEditVariable(IVariable variable)
    {
        if (!TryRemoveEditVariable(variable))
            throw new UnknownEditVariableException();
    }

    public bool HasEditVariable(IVariable variable)
        => GetInfo(variable)?.Edit is not null;

    public void SuggestValue(IVariable variable, double value)
    {
        var variableInfo = GetInfo(variable);
        if (variableInfo?.Edit is null)
            throw new UnknownEditVariableException();

        double delta = value - variableInfo.Edit.Constant;
        variableInfo.Edit.Constant = value;

        {
            if (this.Rows.TryGetValue(variableInfo.Edit.Tag.Marker, out var row))
            {
                if (row.Add(-delta) < 0)
                    this.InfeasibleRows.Add(variableInfo.Edit.Tag.Marker);
                DualOptimize();
                return;
            }

            if (variableInfo.Edit.Tag.Other is { } other && this.Rows.TryGetValue(other, out row))
            {
                if (row.Add(-delta) < 0)
                    this.InfeasibleRows.Add(other);
                DualOptimize();
                return;
            }
        }

        foreach (var (symbol, row) in this.Rows)
        {
            double coefficient = row.GetCoefficientForSymbol(variableInfo.Edit.Tag.Marker);
            if (coefficient != 0 && row.Add(delta * coefficient) < 0 && symbol.Type != SymbolType.External)
                this.InfeasibleRows.Add(symbol);
        }

        DualOptimize();
    }

    public void UpdateVariables()
    {
        foreach (var variableInfo in this.Variables.Values)
            variableInfo.Variable.Value = this.Rows.TryGetValue(variableInfo.Symbol, out var row) ? row.Constant : 0;
    }

    private (Row, Tag) CreateRow(Constraint constraint)
    {
        Row row = new(constraint.Expression.Constant);
        Symbol marker;
        Symbol? other = null;

        foreach (var term in constraint.Expression.Terms)
        {
            if (Util.IsNearZero(term.Coefficient))
                continue;

            var symbol = ObtainInfo(term.Variable).Symbol;
            if (this.Rows.TryGetValue(symbol, out var otherRow))
                row.Insert(otherRow, term.Coefficient);
            else
                row.Insert(symbol, term.Coefficient);
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
                    row.Insert(errorPlus, -1);
                    row.Insert(errorMinus, 1);
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

        if (row.Constant < 0)
            row.ReverseSign();
        return (row, new(marker, other));
    }

    private static Symbol? ChooseSubject(Row row, Tag tag)
    {
        foreach (var symbol in row.Cells.Keys)
            if (symbol.Type == SymbolType.External)
                return symbol;
        if (tag.Marker.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(tag.Marker) < 0)
                return tag.Marker;
        if (tag.Other is { } other && other.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(other) < 0)
                return tag.Other;
        return null;
    }

    private bool AddWithArtificialVariable(Row row)
    {
        Symbol artificial = CreateSymbol(SymbolType.Slack);
        this.Rows[artificial] = new(row);
        this.Artificial = new(row);

        Optimize(this.Artificial);
        bool success = Util.IsNearZero(this.Artificial.Constant);
        this.Artificial = null;

        if (this.Rows.TryGetValue(artificial, out var rowPointer))
        {
            foreach (var (existingSymbol, existingRow) in this.Rows.Dictionary)
                if (existingRow == rowPointer)
                    this.Rows.Remove(existingSymbol);

            if (rowPointer.Cells.Count == 0)
                return success;

            if (GetAnyPivotableSymbol(rowPointer) is not { } entering)
                return false;

            rowPointer.SolveForSymbols(artificial, entering);
            Substitute(entering, rowPointer);
            this.Rows[entering] = rowPointer;
        }

        foreach (var existingRow in this.Rows.Values)
            existingRow.Remove(artificial);
        this.Objective.Remove(artificial);
        return success;
    }

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

    private void Optimize(Row objective)
    {
        while (true)
        {
            if (GetEnteringSymbol(objective) is not { } entering)
                return;

            var entry = GetLeavingRow(entering) ?? throw new InternalSolverException("The objective is unbounded");
            var leaving = this.Rows.FirstOrNull(kvp => kvp.Value == entry)?.Key ?? throw new InternalSolverException();

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

    private static Symbol? GetEnteringSymbol(Row objective)
    {
        foreach (var (symbol, value) in objective.Cells)
            if (symbol.Type != SymbolType.Dummy && value < 0)
                return symbol;
        return null;
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

    private static Symbol? GetAnyPivotableSymbol(Row row)
        => row.Cells.Keys.LastOrNull(s => s.Type is SymbolType.Slack or SymbolType.Error);

    private Row? GetLeavingRow(Symbol entering)
    {
        double ratio = double.MaxValue;
        Row? leavingRow = null;

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
                leavingRow = candidateRow;
            }
        }

        return leavingRow;
    }

    private Symbol CreateSymbol(SymbolType type)
        => new(++this.NextSymbolID, type);

    private static bool AreAllDummies(Row row)
        => row.Cells.Keys.All(s => s.Type == SymbolType.Dummy);

    private VariableInfo? GetInfo(IVariable variable)
        => this.Variables.GetValueOrNull(variable);

    private VariableInfo ObtainInfo(IVariable variable)
    {
        if (!this.Variables.TryGetValue(variable, out var info))
        {
            info = new(variable, CreateSymbol(SymbolType.External));
            this.Variables[variable] = info;
        }
        return info;
    }
}
