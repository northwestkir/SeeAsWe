using BenchmarkDotNet.Running;

namespace SeeAsWee.Benchmarks
{
	class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<SeeAsWeeVsCsvHelper>();
			//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
		}
	}
}