namespace Datadog.Trace.Events;

public sealed class Span : IDisposable
{
    private readonly Tracer _tracer;

    public ulong TraceId { get; }

    public ulong SpanId { get; }

    public ulong ParentId { get; }

    public Span? Parent { get; }

    internal Span(Tracer tracer, ulong traceId, ulong spanId, ulong parentId, Span? parent)
    {
        _tracer = tracer;
        TraceId = traceId;
        SpanId = spanId;
        ParentId = parentId;
        Parent = parent;
    }

    public void AddTag(string name, string value)
    {
        _tracer.AddTag(this, name, value);
    }

    public void AddTag(string name, double value)
    {
        _tracer.AddTag(this, name, value);
    }

    public void Dispose()
    {
        _tracer.FinishSpan(this);
    }
}
