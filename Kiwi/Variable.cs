namespace Nanoray.Kiwi;

public partial record struct Variable(
    IVariableStore Store,
    string? Name = null
)
{
    public readonly double Value
    {
        get => Store.Value;
        set => Store.Value = value;
    }

    public Variable(string? name = null) : this(new VariableStore(), name) { }
}
