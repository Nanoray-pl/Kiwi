using System.Collections.Generic;

namespace Nanoray.Kiwi;

internal sealed class Row
{
    public double Constant { get; set; }
    public Dictionary<Symbol, double> Cells { get; private set; }

    public Row(double constant = 0)
    {
        this.Constant = constant;
        this.Cells = new Dictionary<Symbol, double>();
    }

    public Row(Row other)
    {
        this.Constant = other.Constant;
        this.Cells = new(other.Cells);
    }

    private void SetCell(Symbol symbol, double coefficient)
    {
        if (Util.IsNearZero(coefficient))
            Cells.Remove(symbol);
        else
            Cells[symbol] = coefficient;
    }

    internal double Add(double value)
        => this.Constant += value;

    internal void Insert(Symbol symbol, double coefficient = 1)
        => SetCell(symbol, Cells.GetValueOrDefault(symbol) + coefficient);

    internal void Insert(Row other, double coefficient = 1)
    {
        this.Constant += other.Constant * coefficient;
        foreach (var (symbol, symbolCoefficient) in other.Cells)
            SetCell(symbol, this.Cells.GetValueOrDefault(symbol) + symbolCoefficient * coefficient);
    }

    internal void Remove(Symbol symbol)
        => Cells.Remove(symbol);

    internal void ReverseSign()
    {
        this.Constant = -this.Constant;
        foreach (var (symbol, coefficient) in this.Cells)
            this.Cells[symbol] = -coefficient;
    }

    internal void SolveForSymbol(Symbol symbol)
    {
        if (!this.Cells.TryGetValue(symbol, out double symbolCoefficient))
            throw new InternalSolverException();
        this.Cells.Remove(symbol);

        double coefficient = -1 / symbolCoefficient;
        this.Constant *= coefficient;

        foreach (var (cellSymbol, cellCoefficient) in this.Cells)
            this.Cells[cellSymbol] = cellCoefficient * coefficient;
    }

    internal void SolveForSymbols(Symbol lhs, Symbol rhs)
    {
        Insert(lhs, -1);
        SolveForSymbol(rhs);
    }

    internal double GetCoefficientForSymbol(Symbol symbol)
        => this.Cells.GetValueOrDefault(symbol);

    internal void Substitute(Symbol symbol, Row row)
    {
        if (!this.Cells.TryGetValue(symbol, out double coefficient))
            return;
        this.Cells.Remove(symbol);
        Insert(row, coefficient);
    }
}
