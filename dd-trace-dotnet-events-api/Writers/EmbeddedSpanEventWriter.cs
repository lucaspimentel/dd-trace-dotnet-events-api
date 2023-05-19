using System;
using System.Buffers;
using System.Runtime.InteropServices;
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

    public async ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken = default)
    {
        var stream = MemoryStreamManager.Shared.GetStream();
        await _serializer.SerializeAsync(spanEvents, stream, cancellationToken).ConfigureAwait(false);

        unsafe
        {
            byte[] buffer = stream.GetBuffer();
            MemoryHandle handle = buffer.AsMemory().Pin();
            _ = Submit(buffer.Length, handle.Pointer);
        }
    }

    // [LibraryImport("ffi", EntryPoint = "submit")]
    // private static unsafe partial uint Submit(nint size, void* ptr);

    [DllImport("ffi", EntryPoint = "submit")]
    private static extern unsafe uint Submit(nint size, void* ptr);
}
