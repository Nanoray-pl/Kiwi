using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kiwi.Tests;

var _ = BenchmarkRunner.Run<Benchmarks>;

class Benchmarks
{
    [Benchmark]
    public static void Benchmark()
        => new RealWorldTests().TestGridLayout();
}
