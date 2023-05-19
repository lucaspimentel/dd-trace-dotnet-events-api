using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Datadog.Trace.Agent.Events.Writers;

public interface ISpanEventWriter
{
    ValueTask WriteAsync(ReadOnlyMemory<SpanEvent> spanEvents, CancellationToken cancellationToken = default);
}
