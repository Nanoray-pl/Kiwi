using System;

namespace Nanoray.Kiwi;

public sealed class UnsatisfiableConstraintException : Exception
{
    public Constraint Constraint { get; private set; }

    public UnsatisfiableConstraintException(Constraint constraint) : base($"Unsatisfiable constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
