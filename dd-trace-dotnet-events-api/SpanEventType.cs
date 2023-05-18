namespace Datadog.Trace.Agent.Events;

#nullable enable

public enum SpanEventType : byte
{
    Unknown = 0,
    StartSpan = 4,
    FinishSpan = 5,
    AddSpanTags = 6,
}
