using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.Agent.Events.Serializers;

#nullable enable

namespace Datadog.Trace.Agent.Events.Writers;

public class EmbeddedSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;

    public EmbeddedSpanEventWriter(ISpanEventSerializer serializer)
    {
        _serializer = serializer;
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<SpanEvent> spanEvents, CancellationToken cancellationToken = default)
    {
        var stream = MemoryStreamManager.Shared.GetStream();
        await using var disposable = stream.ConfigureAwait(false);

        await _serializer.SerializeAsync(spanEvents, stream, cancellationToken).ConfigureAwait(false);
        byte[] buffer = stream.GetBuffer();
        MemoryHandle payloadBufferHandle = buffer.AsMemory().Pin();

        string hostName = "http://localhost:8126";
        byte[] hostNameBytes = Encoding.UTF8.GetBytes(hostName);
        MemoryHandle hostnameHandle = hostNameBytes.AsMemory().Pin();

        unsafe
        {
            _ = Submit(buffer.Length, payloadBufferHandle.Pointer, hostNameBytes.Length, hostnameHandle.Pointer);
        }
    }

    // [LibraryImport("ffi", EntryPoint = "submit")]
    // private static unsafe partial uint Submit(nint size, void* ptr);

    [DllImport("ffi", EntryPoint = "submit")]
    private static extern unsafe uint Submit(nint payloadSize, void* payload, nint hostnameSize, void* hostname);
}
