using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using CommonTracerInterfaces;
using Datadog.Trace.Agent.Events;
using Datadog.Trace.Agent.Events.Serializers;
using Datadog.Trace.Agent.Events.Writers;

namespace Benchmarks;

//[ShortRunJob]
[SimpleJob(RunStrategy.Monitoring)]
//[SimpleJob(RunStrategy.Throughput)]
[MemoryDiagnoser]
[MinColumn, MedianColumn, MaxColumn]
[GcServer(true)]
public class Benchmarks
{
    private const int TraceCount = 20;
    private const int SpansPerTrace = 20;
    private const int TagsPerSpan = 20;

    private KeyValuePair<string, string>[] _tags;

    private ISpanEventWriter _eventWriter;

    [GlobalSetup]
    public void Setup()
    {
        _tags = new KeyValuePair<string, string>[TagsPerSpan];

        for (int i = 0; i < TagsPerSpan; i++)
        {
            _tags[i] = new KeyValuePair<string, string>($"key{i:00}", $"value{i + 1:00}");
        }

        var eventSerializer = new MessagePackSpanEventSerializer();
        _eventWriter = new HttpSpanEventWriter(eventSerializer, "http://localhost:8126/v0.1/events");
    }

    [Benchmark(Baseline = true)]
    public async ValueTask SendFullTrace()
    {
        ITracer tracer = new FullTraceTracer(Datadog.Trace.Tracer.Instance);

        for (int traceIndex = 0; traceIndex < TraceCount; traceIndex++)
        {
            CreateTrace(tracer, service: "benchmark-api-v0.4", spansPerTrace: SpansPerTrace, _tags);
        }

        await tracer.FlushAsync();
    }

    [Benchmark]
    public async ValueTask SendEventsTrace()
    {
        ITracer tracer = new EventTracer(new Tracer(_eventWriter));

        for (int traceIndex = 0; traceIndex < TraceCount; traceIndex++)
        {
            CreateTrace(tracer, service: "benchmark-api-events", spansPerTrace: SpansPerTrace, _tags);
        }

        await tracer.FlushAsync();
    }

    private static void CreateTrace(ITracer tracer, string service, int spansPerTrace, KeyValuePair<string, string>[] tags)
    {
        using (var root = tracer.StartSpan("root", "server", service, "root-resource"))
        {
            // add tags to root span
            root.AddTags(tags);

            for (int spanIndex = 1; spanIndex < spansPerTrace; spanIndex++)
            {
                using (var child = tracer.StartSpan("child", "internal", service, "child-resource"))
                {
                    // add tags to child span
                    child.AddTags(tags);
                }
            }
        }
    }
}
