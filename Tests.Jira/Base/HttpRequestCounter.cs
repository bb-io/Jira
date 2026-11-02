using System.Diagnostics;

namespace Tests.Jira.Base;

public sealed class HttpRequestCounter : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
{
    private const string HttpHandlerListenerName = "HttpHandlerDiagnosticListener";
    private const string RequestStartEventName = "System.Net.Http.HttpRequestOut.Start";

    private readonly List<IDisposable> subscriptions = [];
    private int count;

    public int Count => count;

    public HttpRequestCounter()
    {
        subscriptions.Add(DiagnosticListener.AllListeners.Subscribe(this));
    }

    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name == HttpHandlerListenerName)
            subscriptions.Add(listener.Subscribe(this));
    }

    public void OnNext(KeyValuePair<string, object?> diagnosticEvent)
    {
        if (diagnosticEvent.Key == RequestStartEventName)
            Interlocked.Increment(ref count);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void Dispose() => subscriptions.ForEach(subscription => subscription.Dispose());
}
