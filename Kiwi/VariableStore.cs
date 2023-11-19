namespace Nanoray.Kiwi;

/// <summary>Describes a basic storage for a variable's value.</summary>
public sealed class VariableStore : IVariableStore
{
    /// <inheritdoc/>
    public double Value { get; set; }
}
