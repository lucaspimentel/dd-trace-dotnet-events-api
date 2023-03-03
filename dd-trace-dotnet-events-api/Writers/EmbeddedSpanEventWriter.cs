using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Datadog.Trace.Events.Serializers;

namespace Datadog.Trace.Events.Writers;

public partial class EmbeddedSpanEventWriter : ISpanEventWriter
{
    private readonly ISpanEventSerializer _serializer;

    public EmbeddedSpanEventWriter(ISpanEventSerializer serializer)
    {
        _serializer = serializer;
    }

    public ValueTask WriteAsync(Memory<SpanEvent> spanEvents, CancellationToken cancellationToken)
    {
        using var writer = new ArrayPoolBufferWriter<byte>();
        _serializer.Serialize(spanEvents, writer);

        unsafe
        {
            using var handle = writer.WrittenMemory.Pin();
            _ = Submit(writer.WrittenCount, handle.Pointer);
        }

        return ValueTask.CompletedTask;
    }

    [LibraryImport("ffi", EntryPoint = "submit")]
    private static unsafe partial uint Submit(nint size, void* ptr);
}
