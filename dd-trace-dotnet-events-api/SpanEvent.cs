using System;
using System.Collections.Generic;

#nullable enable

namespace Datadog.Trace.Agent.Events;

public abstract record SpanEvent(
    DateTimeOffset Timestamp,
    ulong TraceId,
    ulong SpanId);

public record StartSpanEvent(
        DateTimeOffset Timestamp,
        ulong TraceId,
        ulong SpanId,
        ulong ParentId,
        string Service,
        string Name,
        string Resource,
        string Type,
        ReadOnlyMemory<KeyValuePair<string, string>> Meta,
        ReadOnlyMemory<KeyValuePair<string, double>> Metrics)
    : SpanEvent(Timestamp, TraceId, SpanId);

public record FinishSpanEvent(
        DateTimeOffset Timestamp,
        ulong TraceId,
        ulong SpanId,
        ReadOnlyMemory<KeyValuePair<string, string>> Meta,
        ReadOnlyMemory<KeyValuePair<string, double>> Metrics)
    : SpanEvent(Timestamp, TraceId, SpanId);

public record AddTagsSpanEvent(
        DateTimeOffset Timestamp,
        ulong TraceId,
        ulong SpanId,
        ReadOnlyMemory<KeyValuePair<string, string>> Meta,
        ReadOnlyMemory<KeyValuePair<string, double>> Metrics)
    : SpanEvent(Timestamp, TraceId, SpanId);

public record ErrorSpanEvent(
        DateTimeOffset Timestamp,
        ulong TraceId,
        ulong SpanId,
        string Message,
        string? Type,
        string? Stack)
    : SpanEvent(Timestamp, TraceId, SpanId);
