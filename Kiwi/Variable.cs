namespace Nanoray.Kiwi;

public sealed partial class Variable : IVariable
{
    public string? Name { get; set; }
    public double Value { get; set; }

    public Variable(string? name = null, double value = 0)
    {
        this.Name = name;
        this.Value = value;
    }
}
