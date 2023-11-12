using System;

namespace Nanoray.Kiwi;

public static class Strength
{
    public static readonly double Required = Create(1000, 1000, 1000);
    public static readonly double Strong = Create(1, 0, 0);
    public static readonly double Medium = Create(0, 1, 0);
    public static readonly double Weak = Create(0, 0, 1);

    public static double Create(double a, double b, double c, double w)
        => Math.Max(0, Math.Min(1_000, a * w)) * 1_000_000 + Math.Max(0, Math.Min(1_000, b * w)) * 1_000 + Math.Max(0, Math.Min(1_000, c * w));

    public static double Create(double a, double b, double c)
        => Create(a, b, c, 1);

    public static double Clip(double value)
        => Math.Max(0, Math.Min(Required, value));
}
