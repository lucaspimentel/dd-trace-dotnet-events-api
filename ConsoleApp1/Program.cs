using Datadog.Trace.Events;
using Datadog.Trace.Events.Serializers;
using Datadog.Trace.Events.Writers;

namespace ConsoleApp1;

public static class Program
{
    public static async Task Main()
    {
        var eventSerializer = new MessagePackSpanEventSerializer();
        var eventWriter = new HttpSpanEventWriter(eventSerializer, "http://localhost:8127/v0.1/events");
        // await using FileStream stream = File.Create(@"C:\temp\messagepack-events.bin");
        // var eventWriter = new StreamSpanEventWriter(eventSerializer, stream);
        var tracer = new Tracer(eventWriter);

        using (var span1 = tracer.StartSpan("root", "server", "my-server", "resource 1"))
        {
            span1.AddTag("key1", "value1");
            span1.AddTag("key2", 2.5);

            await Task.Delay(50);

            using (var span2 = tracer.StartSpan("child", "internal", "my-server", "resource 2"))
            {
                span2.AddTag("key2", "value2");

                await Task.Delay(50);
            }

            await Task.Delay(50);

            using (var span3 = tracer.StartSpan("child", "internal", "my-server", "resource 3"))
            {
                span3.AddTag("key3", "value3");

                await Task.Delay(50);
            }

            await Task.Delay(50);
        }

        await tracer.FlushAsync();
        // await stream.FlushAsync();
    }
}
