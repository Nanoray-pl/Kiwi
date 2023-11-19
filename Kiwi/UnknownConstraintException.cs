using System;

namespace Nanoray.Kiwi;

/// <summary>Represents an unknown constraint error in the solver system when expecting one.</summary>
public sealed class UnknownConstraintException : Exception
{
    /// <summary>The unknown constraint.</summary>
    public Constraint Constraint { get; private set; }

    internal UnknownConstraintException(Constraint constraint) : base($"Unknown constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
