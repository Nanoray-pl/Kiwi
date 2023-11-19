namespace Nanoray.Kiwi;

/// <summary>The operator of the constraint.</summary>
public enum RelationalOperator
{
    /// <summary>Both sides of the equation have to be equal.</summary>
    Equal,

    /// <summary>The left side of the inequality has to be less than or equal the right side.</summary>
    LessThanOrEqual,

    /// <summary>The left side of the inequality has to be greater than or equal the right side.</summary>
    GreaterThanOrEqual
}
