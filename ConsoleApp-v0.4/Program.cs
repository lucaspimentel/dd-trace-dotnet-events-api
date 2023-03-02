using CommonTracerInterfaces;
using Datadog.Trace;
using ITracer = CommonTracerInterfaces.ITracer;

const int spansPerTrace = 10;
const int tagsPerSpan = 10;
const string service = "Console-Api-v0.4";

KeyValuePair<string, string>[] tags = new KeyValuePair<string, string>[tagsPerSpan];

for (int i = 0; i < tagsPerSpan; i++)
{
    tags[i] = new KeyValuePair<string, string>($"key{i:00}", $"value{i + 1:00}");
}

ITracer tracer = new FullTraceTracer(Tracer.Instance);

using (var root = tracer.StartSpan("root", "server", service, "root-resource"))
{
    // add tags to root span
    root.AddTags(tags);

    for (int spanIndex = 1; spanIndex < spansPerTrace; spanIndex++)
    {
        using (var child = tracer.StartSpan("child", "internal", service, "child-resource"))
        {
            // add tags to child span
            child.AddTags(tags);
        }
    }
}

await tracer.FlushAsync();
