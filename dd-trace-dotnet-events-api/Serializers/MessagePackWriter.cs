using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using MessagePack;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

internal readonly ref struct MessagePackWriter
{
    private readonly Stream _stream;
    private readonly StringCache _stringCache;

    public MessagePackWriter(Stream stream, StringCache stringCache)
    {
        _stringCache = stringCache;
        _stream = stream;
    }

    public void Write(StartSpanEvent e)
    {
        // 2 top-level items: event type and additional fields
        WriteArrayHeader(2);

        // write event type
        Write((byte)SpanEventType.StartSpan);

        // start array for additional fields
        WriteArrayHeader(10);

        // start time
        Write(e.Timestamp);

        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.ParentId);
        Write(e.Service);
        Write(e.Name);
        Write(e.Resource);
        Write(e.Meta);
        Write(e.Metrics);
        Write(e.Type);
    }

    public void Write(FinishSpanEvent e)
    {
        // 2 top-level items: event type and additional fields
        WriteArrayHeader(2);

        // write event type
        Write((byte)SpanEventType.FinishSpan);

        // start array for additional fields
        WriteArrayHeader(5);

        // end time
        Write(e.Timestamp);

        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.Meta);
        Write(e.Metrics);
    }

    public void Write(AddTagsSpanEvent e)
    {
        // 2 top-level items: event type and additional fields
        WriteArrayHeader(2);

        // write event type
        Write((byte)SpanEventType.AddSpanTags);

        // start array for additional fields
        WriteArrayHeader(4);

        Write(0); // duration, not used yet
        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.Meta);
        Write(e.Metrics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(ReadOnlyMemory<KeyValuePair<string, string>> tags)
    {
        if (tags.Length == 0)
        {
            WriteMapHeader(0);
            return;
        }

        int count = 0;
        var tagsSpan = tags.Span;

        foreach (var tag in tagsSpan)
        {
            if (!string.IsNullOrEmpty(tag.Value))
            {
                count++;
            }
        }

        WriteMapHeader(count);

        foreach ((string key, string value) in tagsSpan)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Write(key);
                Write(value);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(ReadOnlyMemory<KeyValuePair<string, double>> tags)
    {
        if (tags.Length == 0)
        {
            WriteMapHeader(0);
            return;
        }

        WriteMapHeader(tags.Length);

        foreach ((string key, double value) in tags.Span)
        {
            Write(key);
            Write(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(DateTimeOffset value)
    {
        const long nanoSecondsPerTick = 1_000_000 / TimeSpan.TicksPerMillisecond;
        const long unixEpochInTicks = 621355968000000000; // = DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks

        long nanoseconds = (value.Ticks - unixEpochInTicks) * nanoSecondsPerTick;
        Write(nanoseconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(string? value)
    {
        if (_stringCache == null)
        {
            MessagePackBinary.WriteString(_stream, value);
        }
        else if (string.IsNullOrEmpty(value))
        {
            Write(0);
        }
        else
        {
            int stringIndex = _stringCache.TryAdd(value);
            Write(stringIndex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(ulong value)
    {
        MessagePackBinary.WriteUInt64(_stream, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(long value)
    {
        MessagePackBinary.WriteInt64(_stream, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(byte value)
    {
        MessagePackBinary.WriteByte(_stream, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(double value)
    {
        MessagePackBinary.WriteDouble(_stream, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArrayHeader(int count)
    {
        MessagePackBinary.WriteArrayHeader(_stream, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteMapHeader(int count)
    {
        MessagePackBinary.WriteMapHeader(_stream, count);
    }
}
