using System;

namespace Nanoray.Kiwi;

public sealed class DuplicateConstraintException : Exception
{
    public Constraint Constraint { get; private set; }

    public DuplicateConstraintException(Constraint constraint) : base($"Duplicate constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
