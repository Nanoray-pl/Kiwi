using System;

namespace Nanoray.Kiwi;

/// <summary>Represents an error when trying to solve a non-linear expression.</summary>
public sealed class NonLinearExpressionException : Exception
{
    internal NonLinearExpressionException() : base() { }
}
