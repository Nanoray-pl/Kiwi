using System;

namespace Nanoray.Kiwi;

internal readonly struct Symbol : IEquatable<Symbol>, IComparable<Symbol>
{
    internal readonly int ID;
    internal readonly SymbolType Type;

    internal Symbol(int id, SymbolType type)
    {
        this.ID = id;
        this.Type = type;
    }

    public bool Equals(Symbol other)
        => this.ID == other.ID; // not checking for Type, IDs are auto-generated across all types together

    public override bool Equals(object? obj)
        => obj is Symbol symbol && Equals(symbol);

    public override int GetHashCode()
        => this.ID;

    public int CompareTo(Symbol other)
        => this.ID.CompareTo(other.ID);

    public override string ToString()
        => $"{{#{this.ID} {this.Type}}}";
}
