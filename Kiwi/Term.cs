namespace Nanoray.Kiwi;

/// <summary>Describes a single variable component of an expression.</summary>
/// <param name="Variable">The variable.</param>
/// <param name="Coefficient">The coefficient (multiplier) of the variable.</param>
public partial record struct Term(
    Variable Variable,
    double Coefficient = 1
)
{
    /// <summary>The current value of the term.</summary>
    public readonly double Value
        => Variable.Value * Coefficient;
}
