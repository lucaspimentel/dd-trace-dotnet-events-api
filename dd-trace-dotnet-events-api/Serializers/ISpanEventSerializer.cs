using System.Buffers;

namespace Datadog.Trace.Events.Serializers;

public interface ISpanEventSerializer
{
    void Serialize(Memory<SpanEvent> spanEvents, IBufferWriter<byte> writer);
}
