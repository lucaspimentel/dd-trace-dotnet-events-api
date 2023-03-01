namespace Datadog.Trace.Events.Writers;

public interface ISpanEventWriter
{
    ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken);
}
