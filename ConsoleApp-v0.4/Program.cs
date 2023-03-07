using CommonTracerInterfaces;
using Datadog.Trace;
using ITracer = CommonTracerInterfaces.ITracer;

const int tracesPerFlush = 100;
const int spansPerTrace = 10;
const int tagsPerSpan = 10;
const string service = "Console-v0.4";

KeyValuePair<string, string>[] tags = new KeyValuePair<string, string>[tagsPerSpan];

for (int i = 0; i < tagsPerSpan; i++)
{
    tags[i] = new KeyValuePair<string, string>($"key{i:00}", $"value{i + 1:00}");
}

ITracer tracer = new FullTraceTracer(Tracer.Instance);

// create and flush one trace to warm things up
CreateTrace(tracer, service, tags, spansPerTrace);
await tracer.FlushAsync();

Console.WriteLine("Attach profiler and press [ENTER] to generate more traces.");
Console.ReadLine();

while (true)
{
    for (int y = 0; y < tracesPerFlush; y++)
    {
        CreateTrace(tracer, service, tags, spansPerTrace);
    }

    await tracer.FlushAsync();
}

static void CreateTrace(ITracer tracer, string s, KeyValuePair<string, string>[] tags, int spansPerTrace)
{
    using (var root = tracer.StartSpan("root", "server", s, "root-resource"))
    {
        // add tags to root span
        root.AddTags(tags);

        for (int spanIndex = 1; spanIndex < spansPerTrace; spanIndex++)
        {
            using (var child = tracer.StartSpan("child", "internal", s, "child-resource"))
            {
                // add tags to child span
                child.AddTags(tags);
            }
        }
    }
}
