using System;

namespace Nanoray.Kiwi;

/// <summary>Describes various built-in constraint strengths, with the most notable one being <see cref="Strength.Required"/>.</summary>
public static class Strength
{
    /// <summary>The highest possible constraint strength. Constraints with this strength <b>must</b> be satisfied, otherwise <see cref="UnsatisfiableConstraintException"/> will be thrown.</summary>
    public static readonly double Required = 1000;

    /// <summary>A strong constraint strength.</summary>
    public static readonly double Strong = 750;

    /// <summary>A medium constraint strength.</summary>
    public static readonly double Medium = 500;

    /// <summary>A weak constraint strength.</summary>
    public static readonly double Weak = 250;

    /// <summary>The lowest possible constraint strength. Constraints with this strength will be completely ignored.</summary>
    public static readonly double Disabled = 0;

    /// <summary>Clips any constraint strength to the allowed range.</summary>
    /// <param name="value">The constraint strength to clip.</param>
    /// <returns>Clipped constraint strength.</returns>
    public static double Clip(double value)
        => Math.Max(Disabled, Math.Min(Required, value));
}
