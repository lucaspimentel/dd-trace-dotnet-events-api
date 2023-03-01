using CommunityToolkit.HighPerformance.Buffers;
using Datadog.Trace.Events.Serializers;

namespace Datadog.Trace.Events.Writers;

public class StreamSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;
    private readonly Stream _stream;

    public StreamSpanEventWriter(ISpanEventSerializer serializer, Stream stream)
    {
        _serializer = serializer;
        _stream = stream;
    }

    public async ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken)
    {
        using var writer = new ArrayPoolBufferWriter<byte>();
        _serializer.Serialize(spanEvents, writer);

        await _stream.WriteAsync(writer.WrittenMemory, cancellationToken).ConfigureAwait(false);
    }
}
