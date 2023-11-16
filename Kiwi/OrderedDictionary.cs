using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nanoray.Kiwi;

internal sealed class OrderedDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
{
    public List<TKey> Keys { get; init; }
    public Dictionary<TKey, TValue> Dictionary { get; init; }

    public int Count
        => Keys.Count;

    public OrderedDictionary()
    {
        this.Keys = new();
        this.Dictionary = new();
    }

    public OrderedDictionary(OrderedDictionary<TKey, TValue> other)
    {
        this.Keys = new(other.Keys);
        this.Dictionary = new(other.Dictionary);
    }

    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set
        {
            if (!Dictionary.ContainsKey(key))
                Keys.Add(key);
            Dictionary[key] = value;
        }
    }

    public bool ContainsKey(TKey key)
        => Dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => Dictionary.TryGetValue(key, out value);

    public bool Remove(TKey key)
    {
        bool result = Dictionary.Remove(key);
        if (result)
            Keys.Remove(key);
        return result;
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        => Keys;

    public IEnumerable<TValue> Values
        => Dictionary.Values;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => Keys.Select(k => new KeyValuePair<TKey, TValue>(k, Dictionary[k])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
