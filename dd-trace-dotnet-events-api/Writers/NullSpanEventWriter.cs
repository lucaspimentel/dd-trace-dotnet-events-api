using CommunityToolkit.HighPerformance.Buffers;
using Datadog.Trace.Events.Serializers;

namespace Datadog.Trace.Events.Writers;

public class NullSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;

    public NullSpanEventWriter(ISpanEventSerializer serializer)
    {
        _serializer = serializer;
    }

    public ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken)
    {
        using var writer = new ArrayPoolBufferWriter<byte>();
        _serializer.Serialize(spanEvents, writer);

        return ValueTask.CompletedTask;
    }
}
