using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using Datadog.Trace.Events.Serializers;
using Datadog.Trace.Events.Writers;

namespace Benchmarks;

[ShortRunJob]
// [SimpleJob(RunStrategy.Monitoring)]
[MemoryDiagnoser]
[MinColumn, Q1Column, Q3Column, MaxColumn]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[GcServer(true)]
public class Benchmarks
{
    private const int SpansPerTrace = 10;
    private const int TagsPerSpan = 10;

    private KeyValuePair<string, string>[] _tags;

    [GlobalSetup]
    public void Setup()
    {
        _tags = new KeyValuePair<string, string>[TagsPerSpan];

        for (int i = 0; i < TagsPerSpan; i++)
        {
            _tags[i] = new KeyValuePair<string, string>($"key{i:00}", $"value{i + 1:00}");
        }
    }

    [Benchmark(Baseline = true)]
    public async ValueTask SendFullTrace()
    {
        var tracer = new FullTraceTracer(Datadog.Trace.Tracer.Instance);

        CreateTrace(tracer, "full trace chunk", spansPerTrace: SpansPerTrace, tagsPerSpan: TagsPerSpan);
        await tracer.FlushAsync();
    }

    [Benchmark]
    public async ValueTask SendEventsTrace()
    {
        var eventSerializer = new MessagePackSpanEventSerializer();
        var eventWriter = new HttpSpanEventWriter(eventSerializer, "http://localhost:8127/v0.1/events");
        var tracer = new EventTracer(new Datadog.Trace.Events.Tracer(eventWriter));

        CreateTrace(tracer, "events api", spansPerTrace: SpansPerTrace, tagsPerSpan: TagsPerSpan);
        await tracer.FlushAsync();
    }

    private void CreateTrace(ITracer tracer, string tracerType, int spansPerTrace, int tagsPerSpan)
    {
        using (var root = tracer.StartSpan("root", "server", "my-service", "root-resource"))
        {
            root.AddTag("tracer-type", tracerType);

            // add tags to root span
            for (int tagIndex = 0; tagIndex < tagsPerSpan; tagIndex++)
            {
                (string key, string value) = _tags[tagIndex];
                root.AddTag(key, value);
            }

            for (int spanIndex = 1; spanIndex < spansPerTrace; spanIndex++)
            {
                using (var child = tracer.StartSpan("child", "server", "my-service", "child-resource"))
                {
                    // add tags to child span
                    for (int tagIndex = 0; tagIndex < tagsPerSpan; tagIndex++)
                    {
                        (string key, string value) = _tags[tagIndex];
                        child.AddTag(key, value);
                    }
                }
            }
        }
    }
}
