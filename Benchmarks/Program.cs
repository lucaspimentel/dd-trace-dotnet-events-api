// _ = BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

_ = BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmarks.Benchmarks>();

// var benchmarks = new Benchmarks.Benchmarks();
// benchmarks.Setup();
// await benchmarks.SendFullTrace();
// await benchmarks.SendEventsTrace();
