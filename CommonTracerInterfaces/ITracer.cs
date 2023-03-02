namespace CommonTracerInterfaces;

public interface ITracer
{
    ISpan StartSpan(string name, string type, string service, string resource);


    ValueTask FlushAsync();
}

public interface ISpan : IDisposable
{
    void AddTags(ReadOnlyMemory<KeyValuePair<string, string>> tags);
}
