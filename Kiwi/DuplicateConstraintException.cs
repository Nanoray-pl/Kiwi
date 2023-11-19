using System;

namespace Nanoray.Kiwi;

/// <summary>Represents a duplicate constraint error when adding a constraint to a solver system.</summary>
public sealed class DuplicateConstraintException : Exception
{
    /// <summary>The duplicate constraint.</summary>
    public Constraint Constraint { get; private set; }

    internal DuplicateConstraintException(Constraint constraint) : base($"Duplicate constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
