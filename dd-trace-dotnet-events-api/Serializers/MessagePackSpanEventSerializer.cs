using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;
using MessagePack;

namespace Datadog.Trace.Events.Serializers;

public class MessagePackSpanEventSerializer : ISpanEventSerializer
{
    public void Serialize(Memory<SpanEvent> spanEvents, IBufferWriter<byte> bufferWriter)
    {
        var stringCache = new StringCache();

        // add null and empty strings as index 0 and 1
        stringCache.TryAdd("");

        // write events to a separate buffer first while collecting strings
        var eventBufferWriter = new ArrayPoolBufferWriter<byte>();
        var eventWriter = new MessagePackWriter(eventBufferWriter);
        var eventWriterHelper = new MessagePackWriterHelper(in eventWriter, stringCache);

        // start event array
        eventWriterHelper.WriteArrayHeader(spanEvents.Length);

        foreach (var spanEvent in spanEvents.Span)
        {
            switch (spanEvent)
            {
                case StartSpanEvent startSpan:
                    eventWriterHelper.Write(startSpan);
                    break;

                case FinishSpanEvent finishSpan:
                    eventWriterHelper.Write(finishSpan);
                    break;

                case AddTagsSpanEvent tagsSpan:
                    eventWriterHelper.Write(tagsSpan);
                    break;
            }
        }

        eventWriterHelper.Flush();

        var payloadWriter = new MessagePackWriter(bufferWriter);

        // 2 top-level items: string array and events array
        payloadWriter.WriteArrayHeader(2);

        // start string array
        payloadWriter.WriteArrayHeader(stringCache.Count);

        foreach (string? s in stringCache.GetStrings())
        {
            payloadWriter.Write(s);
        }

        // write the raw event array bytes from buffer
        payloadWriter.WriteRaw(eventBufferWriter.WrittenSpan);

        payloadWriter.Flush();
    }
}
