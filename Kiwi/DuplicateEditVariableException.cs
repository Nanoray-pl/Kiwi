using System;

namespace Nanoray.Kiwi;

/// <summary>Represents a duplicate edit variable error when adding an edit variable to a solver system.</summary>
public sealed class DuplicateEditVariableException : Exception
{
    internal DuplicateEditVariableException() : base() { }
}
