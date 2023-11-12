using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

public sealed class Solver
{
    private sealed class Tag
    {
        public Symbol Marker { get; set; } = new();
        public Symbol Other { get; set; } = new();
    }

    private sealed class EditInfo
    {
        public Tag Tag { get; private set; }
        public Constraint Constraint { get; private set; }
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
        public IVariable Variable { get; private set; }
        public Symbol Symbol { get; private set; }
        public EditInfo? Edit { get; set; }

        public VariableInfo(IVariable variable, Symbol? symbol = null, EditInfo? edit = null)
        {
            this.Variable = variable;
            this.Symbol = symbol ?? new(SymbolType.External);
            this.Edit = edit;
        }
    }

    private Dictionary<Constraint, Tag> Constraints { get; set; } = new();
    private Dictionary<Symbol, Row> Rows { get; set; } = new();
    private Dictionary<IVariable, VariableInfo> Variables { get; set; } = new();
    private List<Symbol> InfeasibleRows { get; set; } = new();
    private Row Objective { get; set; } = new();
    private Row? Artificial { get; set; }

    public void AddConstraint(Constraint constraint)
    {
        if (Constraints.ContainsKey(constraint))
            throw new DuplicateConstraintException(constraint);

        Tag tag = new();
        var row = CreateRow(constraint, tag);
        var subject = ChooseSubject(row, tag);

        if (subject.Type == SymbolType.Invalid && AreAllDummies(row))
        {
            if (Util.IsNearZero(row.Constant))
                throw new UnsatisfiableConstraintException(constraint);
            else
                subject = tag.Marker;
        }

        if (subject.Type == SymbolType.Invalid)
        {
            if (!AddWithArtificialVariable(row))
                throw new UnsatisfiableConstraintException(constraint);
        }
        else
        {
            row.SolveForSymbol(subject);
            Substitute(subject, row);
            this.Rows[subject] = row;
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

        if (this.Rows.ContainsKey(tag.Marker))
        {
            this.Rows.Remove(tag.Marker);
        }
        else
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
        else if (tag.Other.Type == SymbolType.Error)
            RemoveMarkerEffects(tag.Other, constraint.Strength);
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
        Constraint constraint = new(new Expression(new List<Term> { term }), RelationalOperator.Equal, strength);

        AddConstraint(constraint);
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

            if (this.Rows.TryGetValue(variableInfo.Edit.Tag.Other, out row))
            {
                if (row.Add(-delta) < 0)
                    this.InfeasibleRows.Add(variableInfo.Edit.Tag.Other);
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

    private Row CreateRow(Constraint constraint, Tag tag)
    {
        Row row = new(constraint.Expression.Constant);

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
                Symbol slack = new(SymbolType.Slack);
                tag.Marker = slack;
                row.Insert(slack, coefficient);
                if (constraint.Strength < Strength.Required)
                {
                    Symbol error = new(SymbolType.Error);
                    tag.Other = error;
                    row.Insert(error, -coefficient);
                    this.Objective.Insert(error, constraint.Strength);
                }
                break;
            case RelationalOperator.Equal:
                if (constraint.Strength < Strength.Required)
                {
                    Symbol errorPlus = new(SymbolType.Error);
                    Symbol errorMinus = new(SymbolType.Error);
                    tag.Marker = errorPlus;
                    tag.Other = errorMinus;
                    row.Insert(errorPlus, -1);
                    row.Insert(errorMinus, 1);
                    this.Objective.Insert(errorPlus, constraint.Strength);
                    this.Objective.Insert(errorMinus, constraint.Strength);
                }
                else
                {
                    Symbol dummy = new(SymbolType.Dummy);
                    tag.Marker = dummy;
                    row.Insert(dummy);
                }
                break;
            default:
                throw new ArgumentException($"Unhandled `RelationalOperator` {constraint.Operator}");
        }

        if (row.Constant < 0)
            row.ReverseSign();
        return row;
    }

    private static Symbol ChooseSubject(Row row, Tag tag)
    {
        foreach (var symbol in row.Cells.Keys)
            if (symbol.Type == SymbolType.External)
                return symbol;
        if (tag.Marker.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(tag.Marker) < 0)
                return tag.Marker;
        if (tag.Other.Type is SymbolType.Slack or SymbolType.Error)
            if (row.GetCoefficientForSymbol(tag.Other) < 0)
                return tag.Other;
        return new();
    }

    private bool AddWithArtificialVariable(Row row)
    {
        // TODO: the Java code has a "TODO check this" here, so... yeah

        Symbol artificial = new(SymbolType.Slack);
        this.Rows[artificial] = new(row);
        this.Artificial = new(row);

        Optimize(this.Artificial);
        bool success = Util.IsNearZero(this.Artificial.Constant);
        this.Artificial = null;

        if (this.Rows.TryGetValue(artificial, out var rowPointer))
        {
            foreach (var (existingSymbol, existingRow) in this.Rows)
                if (existingRow == rowPointer)
                    this.Rows.Remove(existingSymbol);

            if (rowPointer.Cells.Count == 0)
                return success;

            var entering = GetAnyPivotableSymbol(rowPointer);
            if (entering.Type == SymbolType.Invalid)
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
            var entering = GetEnteringSymbol(objective);
            if (entering.Type == SymbolType.Invalid)
                return;

            var entry = GetLeavingRow(entering) ?? throw new InternalSolverException("The objective is unbounded");
            var leaving = this.Rows.FirstOrNull(kvp => kvp.Value == entry)?.Key ?? throw new InternalSolverException();

            this.Rows.Remove(leaving); // TODO: okay wtf, the Java code did the same `for` loop twice above, leading to this
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

            var entering = GetDualEnteringSymbol(row);
            if (entering.Type == SymbolType.Invalid)
                throw new InternalSolverException();

            this.Rows.Remove(leaving);
            row.SolveForSymbols(leaving, entering);
            Substitute(entering, row);
            this.Rows[entering] = row;
        }
    }

    private static Symbol GetEnteringSymbol(Row objective)
    {
        foreach (var (symbol, value) in objective.Cells)
            if (symbol.Type != SymbolType.Dummy && value < 0)
                return symbol;
        return new();
    }

    private Symbol GetDualEnteringSymbol(Row row)
    {
        Symbol entering = new();
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

    private static Symbol GetAnyPivotableSymbol(Row row)
    {
        // TODO: the original code actually returned the last symbol, not the first one, even though it claimed it did - does it matter?
        foreach (var symbol in row.Cells.Keys)
            if (symbol.Type is SymbolType.Slack or SymbolType.Error)
                return symbol;
        return new();
    }

    private Row? GetLeavingRow(Symbol entering)
    {
        double ratio = double.MaxValue;
        Row? leavingRow = null;

        foreach (var (symbol, candidateRow) in this.Rows)
        {
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

    private static bool AreAllDummies(Row row)
        => row.Cells.Keys.All(s => s.Type == SymbolType.Dummy);

    private VariableInfo? GetInfo(IVariable variable)
        => this.Variables.TryGetValue(variable, out var info) ? info : null;

    private VariableInfo ObtainInfo(IVariable variable)
    {
        if (!this.Variables.TryGetValue(variable, out var info))
        {
            info = new(variable);
            this.Variables[variable] = info;
        }
        return info;
    }
}
