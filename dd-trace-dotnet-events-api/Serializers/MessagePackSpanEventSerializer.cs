using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

public class MessagePackSpanEventSerializer : ISpanEventSerializer
{
    public ValueTask SerializeAsync(ReadOnlyMemory<SpanEvent> spanEvents, Stream stream, CancellationToken cancellationToken = default)
    {
        using var stringCache = new StringCache();

        // write events to a separate buffer first while collecting strings
        using var eventBuffer = MemoryStreamManager.Shared.GetStream();
        var eventWriter = new MessagePackWriter(eventBuffer, stringCache);

        foreach (var spanEvent in spanEvents.Span)
        {
            switch (spanEvent)
            {
                case StartSpanEvent e:
                    eventWriter.Write(e);
                    break;

                case FinishSpanEvent e:
                    eventWriter.Write(e);
                    break;

                case AddTagsSpanEvent e:
                    eventWriter.Write(e);
                    break;

                case ErrorSpanEvent e:
                    eventWriter.Write(e);
                    break;
            }
        }

        // write event type
        MessagePackBinary.WriteByte(stream, (byte)SpanEventType.Strings);

        // start string array
        MessagePackBinary.WriteArrayHeader(stream, stringCache.Count);

        foreach (string value in stringCache.GetStrings())
        {
            MessagePackBinary.WriteString(stream, value);
        }

        // write the raw event bytes we serialized earlier
        eventBuffer.Position = 0;
        Task task = eventBuffer.CopyToAsync(stream, cancellationToken);
        return new ValueTask(task);
    }
}
