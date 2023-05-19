using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

public class JsonSpanEventSerializer : ISpanEventSerializer
{
    private readonly JsonSerializerOptions? _options;

    public JsonSpanEventSerializer() : this(null)
    {
    }

    public JsonSpanEventSerializer(JsonSerializerOptions? options)
    {
        _options = options;
    }

    public ValueTask SerializeAsync(Memory<SpanEvent> spanEvents, Stream stream, CancellationToken cancellationToken = default)
    {
        return new ValueTask(JsonSerializer.SerializeAsync(stream, spanEvents, _options, cancellationToken));
    }
}
