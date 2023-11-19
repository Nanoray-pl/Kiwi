namespace Nanoray.Kiwi;

/// <summary>Describes a variable that can be used in a solver system.</summary>
/// <param name="Store">The storage for the variable's value.</param>
/// <param name="Name">An optional name for the variable.</param>
public partial record struct Variable(
    IVariableStore Store,
    string? Name = null
)
{
    /// <summary>The variable's current value.</summary>
    public readonly double Value
    {
        get => Store.Value;
        set => Store.Value = value;
    }

    /// <summary>Describes a variable that can be used in a solver system, with a default basic storage.</summary>
    /// <param name="name">An optional name for the variable.</param>
    public Variable(string? name = null) : this(new VariableStore(), name) { }
}
