using System.Buffers;
using System.Collections.Concurrent;
using Datadog.Trace.Events.Writers;

namespace Datadog.Trace.Events;

public sealed class Tracer
{
    private static readonly string RuntimeId = Guid.NewGuid().ToString();

    private static readonly string Env = Environment.GetEnvironmentVariable("DD_ENV") ?? "";

    private readonly AsyncLocal<Span> _activeSpan = new();
    private readonly List<SpanEvent> _events = new();
    private readonly ConcurrentDictionary<Span, Span> _openSpans = new();
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
        Span parent = _activeSpan.Value;

        if (parent == Span.None)
        {
            traceId = (ulong)Random.Shared.NextInt64();
            parentId = 0;
        }
        else
        {
            traceId = parent.TraceId;
            parentId = parent.SpanId;
        }

        var span = new Span(this, traceId, spanId, parentId);
        PushSpan(span, parent);

        KeyValuePair<string, string>[] meta;
        KeyValuePair<string, double>[] metrics;

        if (parentId == 0)
        {
            meta = new KeyValuePair<string, string>[]
                   {
                       new("language", "dotnet"),
                       new("runtime-id", RuntimeId),
                       new("env", Env)
                   };

            metrics = new KeyValuePair<string, double>[]
                      {
                          new("_dd.tracer_kr", 0),
                          new("_dd.agent_psr", 1),
                          new("_sampling_priority_v1", 1),
                          new("_dd.top_level", 1),
                          new("process_id", Environment.ProcessId),
                      };
        }
        else
        {
            meta = new KeyValuePair<string, string>[]
                   {
                       new("language", "dotnet"),
                       new("env", Env)
                   };

            metrics = Array.Empty<KeyValuePair<string, double>>();
        }

        var spanEvent = new StartSpanEvent(
            traceId,
            spanId,
            parentId,
            DateTimeOffset.UtcNow,
            service,
            name,
            resource,
            type,
            meta,
            metrics);

        lock (_events)
        {
            _events.Add(spanEvent);
        }

        return span;
    }

    internal void FinishSpan(Span span)
    {
        PopSpan(span);

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

    private void PushSpan(Span span, Span parent)
    {
        _activeSpan.Value = span;
        _openSpans.TryAdd(span, parent);
    }

    private void PopSpan(Span span)
    {
        if (_openSpans.TryRemove(span, out var parent))
        {
            // if the active span is finished, set its parent as the new active span.
            // if it didn't have a parent, then there is no active span now.
            if (_activeSpan.Value == span)
            {
                _activeSpan.Value = parent;
            }
        }
    }

    private static KeyValuePair<string, TValue>[] CreateTagsArray<TValue>(string name, TValue value)
    {
        return new[] { new KeyValuePair<string, TValue>(name, value) };
    }
}
