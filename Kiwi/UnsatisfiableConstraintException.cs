using System;

namespace Nanoray.Kiwi;

/// <summary>Represents an unsatisfiable constraint error in the solver system.</summary>
public sealed class UnsatisfiableConstraintException : Exception
{
    /// <summary>The unsatisfiable constraint.</summary>
    public Constraint Constraint { get; private set; }

    internal UnsatisfiableConstraintException(Constraint constraint) : base($"Unsatisfiable constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
