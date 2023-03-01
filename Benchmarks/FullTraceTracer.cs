using System.Threading.Tasks;
using Datadog.Trace;

namespace Benchmarks;

public sealed class FullTraceTracer : ITracer
{
    private readonly Datadog.Trace.Tracer _tracer;

    public FullTraceTracer(Tracer tracer)
    {
        _tracer = tracer;
    }

    public ISpan StartSpan(string name, string type, string service, string resource)
    {
        IScope scope = _tracer.StartActive(name);
        var span = scope.Span;

        span.Type = type;
        span.ServiceName = service;
        span.ResourceName = resource;

        return new FullTraceSpan(span);
    }

    public ValueTask FlushAsync()
    {
        return new ValueTask(_tracer.ForceFlushAsync());
    }
}

public sealed class FullTraceSpan : ISpan
{
    private readonly Datadog.Trace.ISpan _span;

    public FullTraceSpan(Datadog.Trace.ISpan span)
    {
        _span = span;
    }

    public void Dispose()
    {
        _span.Dispose();
    }

    public void AddTag(string name, string value)
    {
        _span.SetTag(name, value);
    }
}
