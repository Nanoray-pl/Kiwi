using System;
using System.Collections.Generic;

namespace Nanoray.Kiwi;

internal sealed class Row
{
    public double Constant { get; set; }
    public Dictionary<Symbol, double> Cells { get; set; }

    public Row(double constant = 0)
    {
        this.Constant = constant;
        this.Cells = new Dictionary<Symbol, double>();
    }

    public Row(Row other)
    {
        this.Constant = other.Constant;
        this.Cells = new Dictionary<Symbol, double>(other.Cells);
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
    {
        if (!Cells.TryGetValue(symbol, out double existingCoefficient))
            coefficient += existingCoefficient;
        SetCell(symbol, coefficient);
    }

    internal void Insert(Row other, double coefficient = 1)
    {
        this.Constant += other.Constant * coefficient;

        foreach (var (symbol, otherSymbolCoefficient) in other.Cells)
        {
            if (!this.Cells.TryGetValue(symbol, out double thisSymbolCoefficient))
                thisSymbolCoefficient = 0;
            double newSymbolCoefficient = thisSymbolCoefficient + otherSymbolCoefficient * coefficient;
            SetCell(symbol, newSymbolCoefficient);
        }
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
            throw new NullReferenceException(); // TODO: improve exceptions; original code didn't handle this, so it's probably not very needed
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
        => this.Cells.TryGetValue(symbol, out double coefficient) ? coefficient : 0;

    internal void Substitute(Symbol symbol, Row row)
    {
        if (!this.Cells.TryGetValue(symbol, out double coefficient))
            return;
        this.Cells.Remove(symbol);
        Insert(row, coefficient);
    }
}
