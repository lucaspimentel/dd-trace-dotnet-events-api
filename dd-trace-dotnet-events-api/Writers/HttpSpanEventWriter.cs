using System;
using System.Buffers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.Agent.Events.Serializers;
using Microsoft.IO;

#nullable enable

namespace Datadog.Trace.Agent.Events.Writers;

public class HttpSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;
    private readonly Uri _uri;
    private readonly HttpClient _client = new();

    public HttpSpanEventWriter(ISpanEventSerializer serializer, string uri)
        : this(serializer, new Uri(uri))
    {
    }

    public HttpSpanEventWriter(ISpanEventSerializer serializer, Uri uri)
    {
        _serializer = serializer;
        _uri = uri;
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<SpanEvent> spanEvents, CancellationToken cancellationToken = default)
    {
        var stream = MemoryStreamManager.Shared.GetStream();
        await _serializer.SerializeAsync(spanEvents, stream, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;

        var content = new StreamContent(stream);
        var response = await _client.PutAsync(_uri, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
