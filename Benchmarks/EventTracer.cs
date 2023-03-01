﻿using System.Threading.Tasks;
using Datadog.Trace.Events;

namespace Benchmarks;

public sealed class EventTracer : ITracer
{
    private readonly Tracer _tracer;

    public EventTracer(Tracer tracer)
    {
        _tracer = tracer;
    }

    public ISpan StartSpan(string name, string type, string service, string resource)
    {
        Span span = _tracer.StartSpan(name, type, service, resource);
        return new EventSpan(span);
    }

    public ValueTask FlushAsync()
    {
        return _tracer.FlushAsync();
    }
}

public sealed class EventSpan : ISpan
{
    private readonly Span _span;

    public EventSpan(Span span)
    {
        _span = span;
    }

    public void Dispose()
    {
        _span.Dispose();
    }

    public void AddTag(string name, string value)
    {
        _span.AddTag(name, value);
    }
}
