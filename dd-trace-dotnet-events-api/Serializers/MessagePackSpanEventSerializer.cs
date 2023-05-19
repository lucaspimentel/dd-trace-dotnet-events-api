using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

public class MessagePackSpanEventSerializer : ISpanEventSerializer
{
    public ValueTask SerializeAsync(Memory<SpanEvent> spanEvents, Stream stream, CancellationToken cancellationToken = default)
    {
        using var stringCache = new StringCache();

        // write events to a separate buffer first while collecting strings
        var eventStream = MemoryStreamManager.Shared.GetStream();
        var eventWriter = new MessagePackWriter(eventStream, stringCache);

        // start event array
        eventWriter.WriteArrayHeader(spanEvents.Length);

        foreach (var spanEvent in spanEvents.Span)
        {
            switch (spanEvent)
            {
                case StartSpanEvent startSpan:
                    eventWriter.Write(startSpan);
                    break;

                case FinishSpanEvent finishSpan:
                    eventWriter.Write(finishSpan);
                    break;

                case AddTagsSpanEvent tagsSpan:
                    eventWriter.Write(tagsSpan);
                    break;
            }
        }

        // 2 top-level items: string array and events array
        MessagePackBinary.WriteArrayHeader(stream, 2);

        // start string array
        MessagePackBinary.WriteArrayHeader(stream, stringCache.Count);

        foreach (string s in stringCache.GetStrings())
        {
            MessagePackBinary.WriteString(stream, s);
        }

        // write the raw event bytes we serialized earlier
        return new ValueTask(eventStream.CopyToAsync(stream, cancellationToken));
    }
}
