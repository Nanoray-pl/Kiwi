using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

internal sealed class Row
{
    internal double Constant => _Constant;
    internal readonly OrderedDictionary<Symbol, double> Cells;

    private double _Constant;

    internal Row(double constant = 0)
    {
        this._Constant = constant;
        this.Cells = new();
    }

    internal Row(Row other)
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

    /// <summary>Add a constant value to the row constant.</summary>
    internal double Add(double value)
        => this._Constant += value;

    /// <summary>Insert a symbol into the row with a given coefficient.</summary>
    /// <remarks>
    /// If the symbol already exists in the row, the coefficient will be
    /// added to the existing coefficient. If the resulting coefficient
    /// is zero, the symbol will be removed from the row.
    /// </remarks>
    internal void Insert(Symbol symbol, double coefficient = 1)
        => SetCell(symbol, Cells.Dictionary.GetValueOrDefault(symbol) + coefficient);

    /// <summary>Insert a row into this row with a given coefficient.</summary>
    /// <remarks>
    /// The constant and the cells of the other row will be multiplied by
    /// the coefficient and added to this row. Any cell with a resulting
    /// coefficient of zero will be removed from the row.
    /// </remarks>
    internal void Insert(Row other, double coefficient = 1)
    {
        this._Constant += other._Constant * coefficient;
        foreach (var (symbol, symbolCoefficient) in other.Cells.Dictionary)
            SetCell(symbol, this.Cells.Dictionary.GetValueOrDefault(symbol) + symbolCoefficient * coefficient);
    }

    /// <summary>Remove the given symbol from the row.</summary>
    internal void Remove(Symbol symbol)
        => this.Cells.Remove(symbol);

    /// <summary>Reverse the sign of the constant and all cells in the row.</summary>
    internal void ReverseSign()
    {
        this._Constant = -this._Constant;
        foreach (var (symbol, coefficient) in this.Cells.Dictionary)
            this.Cells[symbol] = -coefficient;
    }

    /// <summary>Solve the row for the given symbol.</summary>
    /// <remarks>
    /// This method assumes the row is of the form a * x + b * y + c = 0
    /// and (assuming solve for x) will modify the row to represent the
    /// right hand side of x = -b / a * y - c / a. The target symbol will
    /// be removed from the row, and the constant and other cells will
    /// be multiplied by the negative inverse of the target coefficient.
    /// The given symbol *must* exist in the row.
    /// </remarks>
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

    /// <summary>Solve the row for the given symbols.</summary>
    /// <remarks>
    /// This method assumes the row is of the form x = b * y + c and will
    /// solve the row such that y = x / b - c / b. The rhs symbol will be
    /// removed from the row, the lhs added, and the result divided by the
    /// negative inverse of the rhs coefficient.
    /// The lhs symbol *must not* exist in the row, and the rhs symbol
    /// *must* exist in the row.
    /// </remarks>
    internal void SolveForSymbols(Symbol lhs, Symbol rhs)
    {
        Insert(lhs, -1);
        SolveForSymbol(rhs);
    }

    /// <summary>Get the coefficient for the given symbol.</summary>
    /// <remarks>If the symbol does not exist in the row, zero will be returned.</remarks>
    internal double GetCoefficientForSymbol(Symbol symbol)
        => this.Cells.Dictionary.GetValueOrDefault(symbol);

    /// <summary>Substitute a symbol with the data from another row.</summary>
    /// <remarks>
    /// Given a row of the form a * x + b and a substitution of the
    /// form x = 3 * y + c the row will be updated to reflect the
    /// expression 3 * a * y + a* c + b.
    /// If the symbol does not exist in the row, this is a no-op.
    /// </remarks>
    internal void Substitute(Symbol symbol, Row row)
    {
        if (!this.Cells.Dictionary.TryGetValue(symbol, out double coefficient))
            return;
        this.Cells.Remove(symbol);
        Insert(row, coefficient);
    }

    /// <summary>Compute the entering variable for a pivot operation.</summary>
    /// <remarks>
    /// This method will return first symbol in the row which is non-dummy and
    /// has a coefficient less than zero.If no symbol meets the criteria, it
    /// means the objective function is at a minimum, and `nil` is returned.
    /// </remarks>
    internal Symbol? GetEnteringSymbol()
    {
        foreach (var (symbol, value) in this.Cells)
            if (symbol.Type != SymbolType.Dummy && value < 0)
                return symbol;
        return null;
    }

    /// <summary>Get the first Slack or Error symbol in this row.</summary>
    /// <remarks>If no such symbol is present, `nil` will be returned.</remarks>
    internal Symbol? GetAnyPivotableSymbol()
    {
        foreach (var symbol in this.Cells.Keys)
            if (symbol.Type is SymbolType.Slack or SymbolType.Error)
                return symbol;
        return null;
    }

    /// <summary>Test whether this row is composed of all dummy variables.</summary>
    internal bool AreAllDummies()
        => this.Cells.Keys.All(s => s.Type == SymbolType.Dummy);
}
