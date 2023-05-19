using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.Agent.Events.Serializers;

namespace Datadog.Trace.Agent.Events.Writers;

public class StreamSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;
    private readonly Stream _stream;

    public StreamSpanEventWriter(ISpanEventSerializer serializer, Stream stream)
    {
        _serializer = serializer;
        _stream = stream;
    }

    public async ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken = default)
    {
        await _serializer.SerializeAsync(spanEvents, _stream, cancellationToken).ConfigureAwait(false);
    }
}
