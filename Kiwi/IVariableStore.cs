namespace Nanoray.Kiwi;

/// <summary>Describes a storage for a variable's value.</summary>
public interface IVariableStore
{
    /// <summary>The variable's stored value.</summary>
    double Value { get; set; }
}
