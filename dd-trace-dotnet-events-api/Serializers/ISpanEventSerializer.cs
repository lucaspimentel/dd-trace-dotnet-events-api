using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

public interface ISpanEventSerializer
{
    ValueTask SerializeAsync(Memory<SpanEvent> spanEvents, Stream stream, CancellationToken cancellationToken = default);
}
