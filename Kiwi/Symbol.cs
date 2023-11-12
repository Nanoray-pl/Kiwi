namespace Nanoray.Kiwi;

internal sealed class Symbol
{
    public SymbolType Type { get; private set; }

    public Symbol(SymbolType type = SymbolType.Invalid)
    {
        this.Type = type;
    }
}
