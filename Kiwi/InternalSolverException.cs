using System;

namespace Nanoray.Kiwi;

public sealed class InternalSolverException : Exception
{
    internal InternalSolverException(string? message) : base(message) { }

    internal InternalSolverException() : base() { }
}
