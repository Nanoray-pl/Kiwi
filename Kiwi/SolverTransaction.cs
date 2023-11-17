using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanoray.Kiwi;

public sealed class SolverTransaction
{
    internal interface IChange
    {
        public record struct AddConstraint(
            Constraint Constraint
        ) : IChange;

        public record struct RemoveConstraint(
            Constraint Constraint
        ) : IChange;

        public record struct AddEditVariable(
            Variable Variable,
            double Strength
        ) : IChange;

        public record struct RemoveEditVariable(
            Variable Variable
        ) : IChange;

        public record struct SuggestValue(
            Variable Variable,
            double Value
        ) : IChange;

        public record struct ApplyPartial() : IChange;
    }

    private readonly Solver Solver;
    private readonly List<IChange> Changes = new();

    public bool IsCancelled { get; private set; }

    internal SolverTransaction(Solver solver)
    {
        this.Solver = solver;
    }

    public void AddConstraint(Constraint constraint)
        => this.Changes.Add(new IChange.AddConstraint(constraint));

    public void RemoveConstraint(Constraint constraint)
        => this.Changes.Add(new IChange.RemoveConstraint(constraint));

    public void AddEditVariable(Variable variable, double strength)
        => this.Changes.Add(new IChange.AddEditVariable(variable, strength));

    public void RemoveEditVariable(Variable variable)
        => this.Changes.Add(new IChange.RemoveEditVariable(variable));

    public void SuggestValue(Variable variable, double value)
        => this.Changes.Add(new IChange.SuggestValue(variable, value));

    public void ApplyPartial()
    {
        if (this.Changes.Count == 0 || this.Changes.LastOrDefault() is IChange.ApplyPartial)
            return;
        this.Changes.Add(new IChange.ApplyPartial());
    }

    public void Apply(bool updateVariables = true)
    {
        if (this.IsCancelled)
            return;

        bool oldAutoSolve = this.Solver.AutoSolve;
        try
        {
            this.Solver.AutoSolve = false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ActuallyApplyPartial()
            {
                this.Solver.AutoSolve = false;
                this.Solver.AutoSolve = true;
            }

            foreach (var change in this.Changes)
            {
                if (change is IChange.AddConstraint addConstraint)
                    this.Solver.AddConstraint(addConstraint.Constraint);
                else if (change is IChange.RemoveConstraint removeConstraint)
                    this.Solver.RemoveConstraint(removeConstraint.Constraint);
                else if (change is IChange.AddEditVariable addEditVariable)
                    this.Solver.AddEditVariable(addEditVariable.Variable, addEditVariable.Strength);
                else if (change is IChange.RemoveEditVariable removeEditVariable)
                    this.Solver.RemoveEditVariable(removeEditVariable.Variable);
                else if (change is IChange.SuggestValue suggestValue)
                    this.Solver.SuggestValue(suggestValue.Variable, suggestValue.Value);
                else if (change is IChange.ApplyPartial)
                    ActuallyApplyPartial();
                else
                    throw new NotImplementedException($"Unhandled `IChange` {change}");
            }
        }
        finally
        {
            this.Solver.AutoSolve = oldAutoSolve;

            if (updateVariables)
            {
                this.Solver.FlushUnusedVariables();
                this.Solver.UpdateVariables();
            }
        }
    }

    public void Cancel()
        => this.IsCancelled = true;
}
