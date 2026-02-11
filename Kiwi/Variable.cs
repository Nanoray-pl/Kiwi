using System;

namespace Nanoray.Kiwi;

/// <summary>Describes a variable that can be used in a solver system.</summary>
public sealed partial class Variable
{
    /// <summary>The storage for the variable's value.</summary>
    public IVariableStore Store { get; }

    /// <summary>An optional name for the variable.</summary>
    public string? Name { get; set; }

    /// <summary>The variable's current value.</summary>
    public double Value
    {
        get => Store.Value;
        set => Store.Value = value;
    }

    /// <summary>Describes a variable that can be used in a solver system, with a default basic storage.</summary>
    /// <param name="name">An optional name for the variable.</param>
    public Variable(string? name = null) : this(new VariableStore(), name) { }

    /// <summary>Describes a variable that can be used in a solver system.</summary>
    /// <param name="store">The storage for the variable's value.</param>
    /// <param name="name">An optional name for the variable.</param>
    public Variable(IVariableStore store, string? name = null)
    {
        this.Store = store ?? throw new ArgumentNullException(nameof(store));
        this.Name = name;
    }
}
