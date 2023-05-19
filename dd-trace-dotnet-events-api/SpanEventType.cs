namespace Datadog.Trace.Agent.Events;

#nullable enable

public enum SpanEventType : byte
{
    Unknown = 0,
    Error = 2,
    StartSpan = 4,
    FinishSpan = 5,
    AddSpanTags = 6,
    Strings = 7,
}
