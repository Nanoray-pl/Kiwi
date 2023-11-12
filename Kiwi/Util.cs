using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanoray.Kiwi;

internal static class Util
{
    private const double Epsilon = 1.0e-8;

    public static bool IsNearZero(double value)
        => Math.Abs(value) < Epsilon;

    public static T? FirstOrNull<T>(this IEnumerable<T> self) where T : struct
        => self.Select(e => new T?(e)).FirstOrDefault();

    public static T? FirstOrNull<T>(this IEnumerable<T> self, Func<T, bool> predicate) where T : struct
        => self.Where(predicate).Select(e => new T?(e)).FirstOrDefault();
}
