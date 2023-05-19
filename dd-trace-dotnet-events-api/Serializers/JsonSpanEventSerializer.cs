using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

#nullable enable

namespace Datadog.Trace.Agent.Events.Serializers;

public class JsonSpanEventSerializer : ISpanEventSerializer
{
    private readonly JsonSerializer _serializer;

    public JsonSpanEventSerializer() : this(null)
    {
    }

    public JsonSpanEventSerializer(JsonSerializerSettings? settings)
    {
        _serializer = JsonSerializer.CreateDefault(settings);
        _serializer.Formatting = Formatting.Indented;

        _serializer.Converters.Add(new ReadOnlyMemoryKeyValuePairConverter<string, string>());
        _serializer.Converters.Add(new ReadOnlyMemoryKeyValuePairConverter<string, double>());
        _serializer.Converters.Add(new ReadOnlyMemoryConverter<SpanEvent>());
    }

    public ValueTask SerializeAsync(ReadOnlyMemory<SpanEvent> spanEvents, Stream stream, CancellationToken cancellationToken = default)
    {
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        _serializer.Serialize(streamWriter, spanEvents);
        return ValueTask.CompletedTask;
    }

    private class ReadOnlyMemoryConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var memory = (ReadOnlyMemory<T>)value!;

            writer.WriteStartArray();

            foreach (var item in memory.Span)
            {
                serializer.Serialize(writer, item);
            }

            writer.WriteEndArray();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ReadOnlyMemory<T>);
        }
    }

    private class ReadOnlyMemoryKeyValuePairConverter<TKey, TValue> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var memory = (ReadOnlyMemory<KeyValuePair<TKey, TValue>>)value!;

            writer.WriteStartObject();

            foreach (var item in memory.Span)
            {
                writer.WritePropertyName(item.Key!.ToString()!);
                writer.WriteValue(item.Value);
            }

            writer.WriteEndObject();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ReadOnlyMemory<KeyValuePair<TKey, TValue>>);
        }
    }
}
