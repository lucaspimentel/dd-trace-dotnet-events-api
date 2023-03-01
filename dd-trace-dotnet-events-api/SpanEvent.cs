namespace Datadog.Trace.Events;

public abstract record SpanEvent(
    ulong TraceId,
    ulong SpanId);

public record StartSpanEvent(
        ulong TraceId,
        ulong SpanId,
        ulong ParentId,
        DateTimeOffset Timestamp,
        string Service,
        string Name,
        string Resource,
        string Type,
        KeyValuePair<string, string>[] Meta,
        KeyValuePair<string, double>[] Metrics)
    : SpanEvent(TraceId, SpanId);

public record FinishSpanEvent(
        ulong TraceId,
        ulong SpanId,
        DateTimeOffset Timestamp,
        KeyValuePair<string, string>[] Meta,
        KeyValuePair<string, double>[] Metrics)
    : SpanEvent(TraceId, SpanId);

public record AddTagsSpanEvent(
        ulong TraceId,
        ulong SpanId,
        KeyValuePair<string, string>[] Meta,
        KeyValuePair<string, double>[] Metrics)
    : SpanEvent(TraceId, SpanId);
