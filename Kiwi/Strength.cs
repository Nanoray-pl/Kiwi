using System;

namespace Nanoray.Kiwi;

/// <summary>Describes various built-in constraint strengths, with the most notable one being <see cref="Strength.Required"/>.</summary>
public static class Strength
{
    private const double MaxStrength = 1000.0;
    private const double StrongWeight = 1000000.0;
    private const double MediumWeight = 1000.0;
    private const double WeakWeight = 1.0;

    /// <summary>Create a lexicographic strength value.</summary>
    /// <param name="a">Strong component.</param>
    /// <param name="b">Medium component.</param>
    /// <param name="c">Weak component.</param>
    /// <param name="w">Scale factor.</param>
    /// <returns>Weighted strength value.</returns>
    public static double Create(double a, double b, double c, double w = 1.0)
    {
        double result = 0.0;
        result += Math.Max(0.0, Math.Min(MaxStrength, a * w)) * StrongWeight;
        result += Math.Max(0.0, Math.Min(MaxStrength, b * w)) * MediumWeight;
        result += Math.Max(0.0, Math.Min(MaxStrength, c * w)) * WeakWeight;
        return result;
    }

    /// <summary>The highest possible constraint strength. Constraints with this strength <b>must</b> be satisfied, otherwise <see cref="UnsatisfiableConstraintException"/> will be thrown.</summary>
    public static readonly double Required = Create(1000.0, 1000.0, 1000.0);

    /// <summary>A strong constraint strength.</summary>
    public static readonly double Strong = Create(1.0, 0.0, 0.0);

    /// <summary>A medium constraint strength.</summary>
    public static readonly double Medium = Create(0.0, 1.0, 0.0);

    /// <summary>A weak constraint strength.</summary>
    public static readonly double Weak = Create(0.0, 0.0, 1.0);

    /// <summary>The lowest possible constraint strength. Constraints with this strength will be completely ignored.</summary>
    public static readonly double Disabled = 0;

    /// <summary>Clips any constraint strength to the allowed range.</summary>
    /// <param name="value">The constraint strength to clip.</param>
    /// <returns>Clipped constraint strength.</returns>
    public static double Clip(double value)
        => Math.Max(Disabled, Math.Min(Required, value));
}
