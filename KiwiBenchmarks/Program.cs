using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kiwi.Benchmarks;
using Kiwi.Tests;

//var tests = new RealWorldTests();
//for (int i = 0; i < int.MaxValue; i++)
//    tests.TestGridLayout();
//Console.ReadLine();

var _ = BenchmarkRunner.Run<Benchmarks>();
Console.ReadLine();

namespace Kiwi.Benchmarks
{
    public class Benchmarks
    {
        [Benchmark]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmarks")]
        public void Benchmark()
            => new RealWorldTests().TestGridLayout();
    }
}
