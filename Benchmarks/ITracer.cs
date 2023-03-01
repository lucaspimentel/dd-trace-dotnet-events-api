using System;
using System.Threading.Tasks;

namespace Benchmarks;

public interface ITracer
{
    ISpan StartSpan(string name, string type, string service, string resource);


    ValueTask FlushAsync();
}

public interface ISpan : IDisposable
{
    void AddTag(string name, string value);
}
