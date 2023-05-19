#nullable enable
using Microsoft.IO;

namespace Datadog.Trace.Agent.Events;

public static class MemoryStreamManager
{
    public static readonly RecyclableMemoryStreamManager Shared = new();
}
