using System;
using System.Buffers.Binary;
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
    private readonly byte[] _buffer = new byte[16];

    public MessagePackWriter(Stream stream, StringCache stringCache)
    {
        _stringCache = stringCache;
        _stream = stream;
    }

    public void Write(StartSpanEvent e)
    {
        // write event type
        Write((byte)SpanEventType.StartSpan);

        // start array for fields
        WriteArrayHeader(10);

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
        // write event type
        Write((byte)SpanEventType.FinishSpan);

        // start array for fields
        WriteArrayHeader(5);

        Write(e.Timestamp);
        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.Meta);
        Write(e.Metrics);
    }

    public void Write(AddTagsSpanEvent e)
    {
        // write event type
        Write((byte)SpanEventType.AddSpanTags);

        // start array for fields
        WriteArrayHeader(5);

        Write(e.Timestamp);
        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.Meta);
        Write(e.Metrics);
    }

    public void Write(ErrorSpanEvent e)
    {
        // write event type
        Write((byte)SpanEventType.Error);

        // start array for fields
        WriteArrayHeader(6);

        Write(e.Timestamp);
        Write(e.TraceId);
        Write(e.SpanId);
        Write(e.Message);
        Write(e.Type);
        Write(e.Stack);
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
        const ulong nanoSecondsPerTick = 1_000_000 / TimeSpan.TicksPerMillisecond;
        const ulong unixEpochInTicks = 621355968000000000; // = DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks

        ulong nanoseconds = ((ulong)value.Ticks - unixEpochInTicks) * nanoSecondsPerTick;
        Write(nanoseconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Write(0);
        }
        else
        {
            var stringIndex = (ulong)_stringCache.TryAdd(value);
            Write(stringIndex + 1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteAsBinary(ulong upper, ulong lower)
    {
        byte[] buffer = _buffer;
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(0, 8), upper);
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(8, 8), lower);
        MessagePackBinary.WriteBytes(_stream, buffer, 0, 16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteAsBinary(ulong value)
    {
        byte[] buffer = _buffer;
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        MessagePackBinary.WriteBytes(_stream, buffer, 0, 8);
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
    private void WriteArrayHeader(int count)
    {
        MessagePackBinary.WriteArrayHeader(_stream, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteMapHeader(int count)
    {
        MessagePackBinary.WriteMapHeader(_stream, count);
    }
}
