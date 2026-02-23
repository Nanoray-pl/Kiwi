using System;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

internal static class Util
{
    private const double Epsilon = 1.0e-8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNearZero(double value)
        => Math.Abs(value) < Epsilon;
}
