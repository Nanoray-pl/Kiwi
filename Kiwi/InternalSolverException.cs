using System;

namespace Nanoray.Kiwi;

/// <summary>Represents an internal solver system error.</summary>
public sealed class InternalSolverException : Exception
{
    internal InternalSolverException(string? message) : base(message) { }

    internal InternalSolverException() : base() { }
}
