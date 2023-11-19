using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

internal static class Util
{
    private const double Epsilon = 1.0e-8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNearZero(double value)
        => Math.Abs(value) < Epsilon;
}

internal static class StructExt
{
    internal static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) where TValue : struct
        => dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    internal static TValue? GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct
        => dictionary.TryGetValue(key, out var value) ? value : null;
}

internal static class ClassExt
{
    internal static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) where TValue : class
        => dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    internal static TValue? GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        => dictionary.TryGetValue(key, out var value) ? value : null;
}
