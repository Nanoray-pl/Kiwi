using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

internal sealed class Row
{
    public double Constant => _Constant;
    public readonly OrderedDictionary<Symbol, double> Cells;

    private double _Constant;

    public Row(double constant = 0)
    {
        this._Constant = constant;
        this.Cells = new();
    }

    public Row(Row other)
    {
        this._Constant = other._Constant;
        this.Cells = new(other.Cells);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCell(Symbol symbol, double coefficient)
    {
        if (Util.IsNearZero(coefficient))
            Cells.Remove(symbol);
        else
            Cells[symbol] = coefficient;
    }

    internal double Add(double value)
        => this._Constant += value;

    internal void Insert(Symbol symbol, double coefficient = 1)
        => SetCell(symbol, Cells.Dictionary.GetValueOrDefault(symbol) + coefficient);

    internal void Insert(Row other, double coefficient = 1)
    {
        this._Constant += other._Constant * coefficient;
        foreach (var (symbol, symbolCoefficient) in other.Cells.Dictionary)
            SetCell(symbol, this.Cells.Dictionary.GetValueOrDefault(symbol) + symbolCoefficient * coefficient);
    }

    internal void Remove(Symbol symbol)
        => this.Cells.Remove(symbol);

    internal void ReverseSign()
    {
        this._Constant = -this._Constant;
        foreach (var (symbol, coefficient) in this.Cells.Dictionary)
            this.Cells[symbol] = -coefficient;
    }

    internal void SolveForSymbol(Symbol symbol)
    {
        if (!this.Cells.Dictionary.TryGetValue(symbol, out double symbolCoefficient))
            throw new InternalSolverException();
        this.Cells.Remove(symbol);

        double coefficient = -1 / symbolCoefficient;
        this._Constant *= coefficient;

        foreach (var (cellSymbol, cellCoefficient) in this.Cells.Dictionary)
            this.Cells[cellSymbol] = cellCoefficient * coefficient;
    }

    internal void SolveForSymbols(Symbol lhs, Symbol rhs)
    {
        Insert(lhs, -1);
        SolveForSymbol(rhs);
    }

    internal double GetCoefficientForSymbol(Symbol symbol)
        => this.Cells.Dictionary.GetValueOrDefault(symbol);

    internal void Substitute(Symbol symbol, Row row)
    {
        if (!this.Cells.Dictionary.TryGetValue(symbol, out double coefficient))
            return;
        this.Cells.Remove(symbol);
        Insert(row, coefficient);
    }

    internal Symbol? GetEnteringSymbol()
    {
        foreach (var (symbol, value) in this.Cells)
            if (symbol.Type != SymbolType.Dummy && value < 0)
                return symbol;
        return null;
    }

    internal Symbol? GetAnyPivotableSymbol()
    {
        foreach (var symbol in this.Cells.Keys)
            if (symbol.Type is SymbolType.Slack or SymbolType.Error)
                return symbol;
        return null;
    }

    internal bool AreAllDummies()
        => this.Cells.Keys.All(s => s.Type == SymbolType.Dummy);
}
