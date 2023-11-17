using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

internal static class Util
{
    private const double Epsilon = 1.0e-8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearZero(double value)
        => Math.Abs(value) < Epsilon;
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
