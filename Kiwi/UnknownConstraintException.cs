using System;

namespace Nanoray.Kiwi;

public sealed class UnknownConstraintException : Exception
{
    public Constraint Constraint { get; private set; }

    public UnknownConstraintException(Constraint constraint) : base($"Unknown constraint {constraint}")
    {
        this.Constraint = constraint;
    }
}
