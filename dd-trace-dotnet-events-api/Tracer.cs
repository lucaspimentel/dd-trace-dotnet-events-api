using System.Buffers;
using Datadog.Trace.Events.Writers;

namespace Datadog.Trace.Events;

public sealed class Tracer
{
    private readonly AsyncLocal<Span?> _activeSpan = new();
    private readonly List<SpanEvent> _events = new();
    private readonly ISpanEventWriter _writer;

    public Tracer(ISpanEventWriter writer)
    {
        _writer = writer;
    }

    public Span StartSpan(
        string name,
        string type,
        string service,
        string resource)
    {
        ulong traceId;
        ulong spanId = (ulong)Random.Shared.NextInt64();
        ulong parentId;

        if (_activeSpan.Value is { TraceId: > 0, SpanId: > 0 } parent)
        {
            traceId = parent.TraceId;
            parentId = parent.SpanId;
        }
        else
        {
            traceId = (ulong)Random.Shared.NextInt64();
            parentId = 0;
        }

        var span = new Span(this, traceId, spanId, parentId, _activeSpan.Value);
        _activeSpan.Value = span;

        var spanEvent = new StartSpanEvent(
            traceId,
            spanId,
            parentId,
            DateTimeOffset.UtcNow,
            service,
            name,
            resource,
            type,
            Array.Empty<KeyValuePair<string, string>>(),
            Array.Empty<KeyValuePair<string, double>>());

        lock (_events)
        {
            _events.Add(spanEvent);
        }

        return span;
    }

    internal void FinishSpan(Span span)
    {
        if (_activeSpan.Value == span)
        {
            // if the active span is finished, set its parent as the new active span.
            // if it didn't have a parent, then there is no active span now.
            _activeSpan.Value = span.Parent;
        }

        var spanEvent = new FinishSpanEvent(
            span.TraceId,
            span.SpanId,
            DateTimeOffset.UtcNow,
            Array.Empty<KeyValuePair<string, string>>(),
            Array.Empty<KeyValuePair<string, double>>()
        );

        lock (_events)
        {
            _events.Add(spanEvent);
        }
    }

    internal void AddTag(Span span, string name, string value)
    {
        var spanEvent = new AddTagsSpanEvent(
            span.TraceId,
            span.SpanId,
            CreateTagsArray(name, value),
            Array.Empty<KeyValuePair<string, double>>()
        );

        lock (_events)
        {
            _events.Add(spanEvent);
        }
    }

    internal void AddTag(Span span, string name, double value)
    {
        var spanEvent = new AddTagsSpanEvent(
            span.TraceId,
            span.SpanId,
            Array.Empty<KeyValuePair<string, string>>(),
            CreateTagsArray(name, value)
        );

        lock (_events)
        {
            _events.Add(spanEvent);
        }
    }

    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        SpanEvent[] events;
        int eventCount;

        lock (_events)
        {
            eventCount = _events.Count;
            events = ArrayPool<SpanEvent>.Shared.Rent(eventCount);
            _events.CopyTo(events);
            _events.Clear();
        }

        await _writer.WriteAsync(events.AsMemory(0, eventCount), cancellationToken).ConfigureAwait(false);

        Array.Clear(events);
        ArrayPool<SpanEvent>.Shared.Return(events);
    }

    private static KeyValuePair<string, TValue>[] CreateTagsArray<TValue>(string name, TValue value)
    {
        return new[] { new KeyValuePair<string, TValue>(name, value) };
    }
}
