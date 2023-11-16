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

    public static T? LastOrNull<T>(this IEnumerable<T> self) where T : struct
        => self.Select(e => new T?(e)).LastOrDefault();

    public static T? LastOrNull<T>(this IEnumerable<T> self, Func<T, bool> predicate) where T : struct
        => self.Where(predicate).Select(e => new T?(e)).LastOrDefault();
}

internal static class StructExt
{
    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) where TValue : struct
        => dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    public static TValue? GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct
        => dictionary.TryGetValue(key, out var value) ? value : null;
}

internal static class ClassExt
{
    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) where TValue : class
        => dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    public static TValue? GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        => dictionary.TryGetValue(key, out var value) ? value : null;
}
