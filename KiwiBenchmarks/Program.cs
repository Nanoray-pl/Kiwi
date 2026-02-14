using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kiwi.Benchmarks;
using Nanoray.Kiwi;

var _ = BenchmarkRunner.Run<EnamlLikeBenchmarks>();

namespace Kiwi.Benchmarks
{
    public class EnamlLikeBenchmarks
    {
        private static readonly Size[] Sizes = new[]
        {
            //new Size(400, 600),
            //new Size(600, 400),
            //new Size(800, 1200),
            //new Size(1200, 800),
            //new Size(400, 800),
            new Size(800, 400)
        };

        private Solver? Solver;
        private Variable? Width;
        private Variable? Height;

        [ParamsSource(nameof(SizeCases))]
        public Size Size { get; set; }

        public IEnumerable<Size> SizeCases()
            => Sizes;

        [GlobalSetup]
        public void Setup()
        {
            Solver = new Solver();
            Width = new Variable("width");
            Height = new Variable("height");
            BuildSolver(Solver, Width, Height);
        }

        [Benchmark]
        public void BuildSolverBenchmark()
        {
            var solver = new Solver();
            var width = new Variable("width");
            var height = new Variable("height");
            BuildSolver(solver, width, height);
            GC.KeepAlive(solver);
        }

        [Benchmark]
        public void SuggestValueBenchmark()
        {
            var solver = Solver ?? throw new InvalidOperationException("Benchmark setup did not run.");
            var width = Width ?? throw new InvalidOperationException("Benchmark setup did not run.");
            var height = Height ?? throw new InvalidOperationException("Benchmark setup did not run.");

            solver.SuggestValue(width, Size.Width);
            solver.SuggestValue(height, Size.Height);
            solver.UpdateVariables();
        }

        private static void BuildSolver(Solver solver, Variable width, Variable height)
        {
            double mmedium = Strength.Create(0.0, 1.0, 0.0, 1.25);
            double smedium = Strength.Create(0.0, 100.0, 0.0);

            var left = new Variable("left");
            var top = new Variable("top");
            var contentsTop = new Variable("contents_top");
            var contentsBottom = new Variable("contents_bottom");
            var contentsLeft = new Variable("contents_left");
            var contentsRight = new Variable("contents_right");
            var midline = new Variable("midline");
            var ctleft = new Variable("ctleft");
            var ctheight = new Variable("ctheight");
            var cttop = new Variable("cttop");
            var ctwidth = new Variable("ctwidth");
            var lb1left = new Variable("lb1left");
            var lb1height = new Variable("lb1height");
            var lb1top = new Variable("lb1top");
            var lb1width = new Variable("lb1width");
            var lb2left = new Variable("lb2left");
            var lb2height = new Variable("lb2height");
            var lb2top = new Variable("lb2top");
            var lb2width = new Variable("lb2width");
            var lb3left = new Variable("lb3left");
            var lb3height = new Variable("lb3height");
            var lb3top = new Variable("lb3top");
            var lb3width = new Variable("lb3width");
            var fl1left = new Variable("fl1left");
            var fl1height = new Variable("fl1height");
            var fl1top = new Variable("fl1top");
            var fl1width = new Variable("fl1width");
            var fl2left = new Variable("fl2left");
            var fl2height = new Variable("fl2height");
            var fl2top = new Variable("fl2top");
            var fl2width = new Variable("fl2width");
            var fl3left = new Variable("fl3left");
            var fl3height = new Variable("fl3height");
            var fl3top = new Variable("fl3top");
            var fl3width = new Variable("fl3width");

            solver.AddEditVariable(width, Strength.Strong);
            solver.AddEditVariable(height, Strength.Strong);

            Constraint[] constraints =
            {
                Constraint.GreaterEqual(left, 0, Strength.Required),
                Constraint.Equal(height + 0, 0, Strength.Medium),
                Constraint.GreaterEqual(top, 0, Strength.Required),
                Constraint.GreaterEqual(width, 0, Strength.Required),
                Constraint.GreaterEqual(height, 0, Strength.Required),
                Constraint.Equal(-top + contentsTop + -10, 0, Strength.Required),
                Constraint.Equal(lb3height + -16, 0, Strength.Strong),
                Constraint.GreaterEqual(lb3height + -16, 0, Strength.Strong),
                Constraint.GreaterEqual(ctleft, 0, Strength.Required),
                Constraint.GreaterEqual(cttop, 0, Strength.Required),
                Constraint.GreaterEqual(ctwidth, 0, Strength.Required),
                Constraint.GreaterEqual(ctheight, 0, Strength.Required),
                Constraint.GreaterEqual(fl3left, 0, Strength.Required),
                Constraint.GreaterEqual(ctheight + -24, 0, smedium),
                Constraint.LessEqual(ctwidth + -1.67772e+07, 0, smedium),
                Constraint.LessEqual(ctheight + -24, 0, smedium),
                Constraint.GreaterEqual(fl3top, 0, Strength.Required),
                Constraint.GreaterEqual(fl3width, 0, Strength.Required),
                Constraint.GreaterEqual(fl3height, 0, Strength.Required),
                Constraint.Equal(lb1width + -67, 0, Strength.Weak),
                Constraint.GreaterEqual(lb2width, 0, Strength.Required),
                Constraint.GreaterEqual(lb2height, 0, Strength.Required),
                Constraint.GreaterEqual(fl2height, 0, Strength.Required),
                Constraint.GreaterEqual(lb3left, 0, Strength.Required),
                Constraint.GreaterEqual(fl2width + -125, 0, Strength.Strong),
                Constraint.Equal(fl2height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(fl2height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(lb3top, 0, Strength.Required),
                Constraint.GreaterEqual(lb3width, 0, Strength.Required),
                Constraint.GreaterEqual(fl1left, 0, Strength.Required),
                Constraint.GreaterEqual(fl1width, 0, Strength.Required),
                Constraint.GreaterEqual(lb1width + -67, 0, Strength.Strong),
                Constraint.GreaterEqual(fl2left, 0, Strength.Required),
                Constraint.Equal(lb2width + -66, 0, Strength.Weak),
                Constraint.GreaterEqual(lb2width + -66, 0, Strength.Strong),
                Constraint.Equal(lb2height + -16, 0, Strength.Strong),
                Constraint.GreaterEqual(fl1height, 0, Strength.Required),
                Constraint.GreaterEqual(fl1top, 0, Strength.Required),
                Constraint.GreaterEqual(lb2top, 0, Strength.Required),
                Constraint.Equal(-lb2top + lb3top + -lb2height + -10, 0, mmedium),
                Constraint.GreaterEqual(-lb3top + -lb3height + fl3top + -10, 0, Strength.Required),
                Constraint.Equal(-lb3top + -lb3height + fl3top + -10, 0, mmedium),
                Constraint.Equal(contentsBottom + -fl3height + -fl3top + -0, 0, mmedium),
                Constraint.GreaterEqual(fl1top + -contentsTop + 0, 0, Strength.Required),
                Constraint.Equal(fl1top + -contentsTop + 0, 0, mmedium),
                Constraint.GreaterEqual(contentsBottom + -fl3height + -fl3top + -0, 0, Strength.Required),
                Constraint.Equal(-left + -width + contentsRight + 10, 0, Strength.Required),
                Constraint.Equal(-top + -height + contentsBottom + 10, 0, Strength.Required),
                Constraint.Equal(-left + contentsLeft + -10, 0, Strength.Required),
                Constraint.Equal(lb3left + -contentsLeft + 0, 0, mmedium),
                Constraint.Equal(fl1left + -midline + 0, 0, Strength.Strong),
                Constraint.Equal(fl2left + -midline + 0, 0, Strength.Strong),
                Constraint.Equal(ctleft + -midline + 0, 0, Strength.Strong),
                Constraint.Equal(fl1top + 0.5 * fl1height + -lb1top + -0.5 * lb1height + 0, 0, Strength.Strong),
                Constraint.GreaterEqual(lb1left + -contentsLeft + 0, 0, Strength.Required),
                Constraint.Equal(lb1left + -contentsLeft + 0, 0, mmedium),
                Constraint.GreaterEqual(-lb1left + fl1left + -lb1width + -10, 0, Strength.Required),
                Constraint.Equal(-lb1left + fl1left + -lb1width + -10, 0, mmedium),
                Constraint.GreaterEqual(-fl1left + contentsRight + -fl1width + -0, 0, Strength.Required),
                Constraint.Equal(width + 0, 0, Strength.Medium),
                Constraint.GreaterEqual(-fl1top + fl2top + -fl1height + -10, 0, Strength.Required),
                Constraint.Equal(-fl1top + fl2top + -fl1height + -10, 0, mmedium),
                Constraint.GreaterEqual(cttop + -fl2top + -fl2height + -10, 0, Strength.Required),
                Constraint.GreaterEqual(-ctheight + -cttop + fl3top + -10, 0, Strength.Required),
                Constraint.GreaterEqual(contentsBottom + -fl3height + -fl3top + -0, 0, Strength.Required),
                Constraint.Equal(cttop + -fl2top + -fl2height + -10, 0, mmedium),
                Constraint.Equal(-fl1left + contentsRight + -fl1width + -0, 0, mmedium),
                Constraint.Equal(-lb2top + -0.5 * lb2height + fl2top + 0.5 * fl2height + 0, 0, Strength.Strong),
                Constraint.GreaterEqual(-contentsLeft + lb2left + 0, 0, Strength.Required),
                Constraint.Equal(-contentsLeft + lb2left + 0, 0, mmedium),
                Constraint.GreaterEqual(fl2left + -lb2width + -lb2left + -10, 0, Strength.Required),
                Constraint.Equal(-ctheight + -cttop + fl3top + -10, 0, mmedium),
                Constraint.Equal(contentsBottom + -fl3height + -fl3top + -0, 0, mmedium),
                Constraint.GreaterEqual(lb1top, 0, Strength.Required),
                Constraint.GreaterEqual(lb1width, 0, Strength.Required),
                Constraint.GreaterEqual(lb1height, 0, Strength.Required),
                Constraint.Equal(fl2left + -lb2width + -lb2left + -10, 0, mmedium),
                Constraint.Equal(-fl2left + -fl2width + contentsRight + -0, 0, mmedium),
                Constraint.GreaterEqual(-fl2left + -fl2width + contentsRight + -0, 0, Strength.Required),
                Constraint.GreaterEqual(lb3left + -contentsLeft + 0, 0, Strength.Required),
                Constraint.GreaterEqual(lb1left, 0, Strength.Required),
                Constraint.Equal(0.5 * ctheight + cttop + -lb3top + -0.5 * lb3height + 0, 0, Strength.Strong),
                Constraint.GreaterEqual(ctleft + -lb3left + -lb3width + -10, 0, Strength.Required),
                Constraint.GreaterEqual(-ctwidth + -ctleft + contentsRight + -0, 0, Strength.Required),
                Constraint.Equal(ctleft + -lb3left + -lb3width + -10, 0, mmedium),
                Constraint.GreaterEqual(fl3left + -contentsLeft + 0, 0, Strength.Required),
                Constraint.Equal(fl3left + -contentsLeft + 0, 0, mmedium),
                Constraint.Equal(-ctwidth + -ctleft + contentsRight + -0, 0, mmedium),
                Constraint.Equal(-fl3left + contentsRight + -fl3width + -0, 0, mmedium),
                Constraint.GreaterEqual(-contentsTop + lb1top + 0, 0, Strength.Required),
                Constraint.Equal(-contentsTop + lb1top + 0, 0, mmedium),
                Constraint.GreaterEqual(-fl3left + contentsRight + -fl3width + -0, 0, Strength.Required),
                Constraint.GreaterEqual(lb2top + -lb1top + -lb1height + -10, 0, Strength.Required),
                Constraint.GreaterEqual(-lb2top + lb3top + -lb2height + -10, 0, Strength.Required),
                Constraint.Equal(lb2top + -lb1top + -lb1height + -10, 0, mmedium),
                Constraint.Equal(fl1height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(fl1height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(lb2left, 0, Strength.Required),
                Constraint.GreaterEqual(lb2height + -16, 0, Strength.Strong),
                Constraint.GreaterEqual(fl2top, 0, Strength.Required),
                Constraint.GreaterEqual(fl2width, 0, Strength.Required),
                Constraint.GreaterEqual(lb1height + -16, 0, Strength.Strong),
                Constraint.Equal(lb1height + -16, 0, Strength.Strong),
                Constraint.GreaterEqual(fl3width + -125, 0, Strength.Strong),
                Constraint.Equal(fl3height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(fl3height + -21, 0, Strength.Strong),
                Constraint.GreaterEqual(lb3height, 0, Strength.Required),
                Constraint.GreaterEqual(ctwidth + -119, 0, smedium),
                Constraint.Equal(lb3width + -24, 0, Strength.Weak),
                Constraint.GreaterEqual(lb3width + -24, 0, Strength.Strong),
                Constraint.GreaterEqual(fl1width + -125, 0, Strength.Strong)
            };

            foreach (var constraint in constraints)
                solver.AddConstraint(constraint);
        }
    }

    public readonly record struct Size(int Width, int Height)
    {
        public override string ToString()
            => $"{Width}x{Height}";
    }
}
