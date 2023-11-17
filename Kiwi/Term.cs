namespace Nanoray.Kiwi;

public partial record struct Term(
    IVariable Variable,
    double Coefficient = 1
)
{
    public readonly double Value
        => Variable.Value * Coefficient;
}
