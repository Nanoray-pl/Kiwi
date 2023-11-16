using System;

namespace Nanoray.Kiwi;

internal readonly struct Symbol : IEquatable<Symbol>
{
    public readonly int ID;
    public readonly SymbolType Type;

    public Symbol(int id, SymbolType type)
    {
        this.ID = id;
        this.Type = type;
    }

    public bool Equals(Symbol other)
        => ID == other.ID; // not checking for Type, IDs are auto-generated across all types together

    public override bool Equals(object? obj)
        => obj is Symbol symbol && Equals(symbol);

    public override int GetHashCode()
        => ID;

    public override string ToString()
        => $"{{#{ID} {Type}}}";
}
