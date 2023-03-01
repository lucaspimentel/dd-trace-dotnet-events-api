using System.Threading.Tasks;
using Datadog.Trace;

namespace Benchmarks;

public sealed class FullTraceTracer : ITracer
{
    private readonly Tracer _tracer;

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

        return new FullTraceSpan(scope);
    }

    public ValueTask FlushAsync()
    {
        return new ValueTask(_tracer.ForceFlushAsync());
    }
}

public sealed class FullTraceSpan : ISpan
{
    private readonly IScope _scope;

    public FullTraceSpan(IScope scope)
    {
        _scope = scope;
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    public void AddTag(string name, string value)
    {
        _scope.Span.SetTag(name, value);
    }
}
