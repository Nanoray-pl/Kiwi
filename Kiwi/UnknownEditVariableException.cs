using System;

namespace Nanoray.Kiwi;

/// <summary>Represents an unknown edit variable error in the solver system when expecting one.</summary>
public sealed class UnknownEditVariableException : Exception
{
    internal UnknownEditVariableException() : base() { }
}
