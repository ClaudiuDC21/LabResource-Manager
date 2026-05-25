using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(LabResource.VerticalSlice.Benchmarks.ReturnAssetBenchmark).Assembly).Run(args);