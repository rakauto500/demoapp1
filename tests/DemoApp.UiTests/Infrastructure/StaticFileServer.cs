using System.Net;
using System.Net.Sockets;

namespace DemoApp.UiTests.Infrastructure;

/// <summary>
/// Tiny HTTP server that serves the demo web app from disk on a random free
/// localhost port, so the suite is self-contained and needs no external host.
/// </summary>
public sealed class StaticFileServer : IDisposable
{
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".js"] = "application/javascript; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".png"] = "image/png",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
    };

    private readonly string _root;
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;

    public StaticFileServer(string rootDirectory)
    {
        _root = Path.GetFullPath(rootDirectory);
        if (!Directory.Exists(_root))
        {
            throw new DirectoryNotFoundException($"Web root not found: {_root}");
        }

        var port = GetFreePort();
        BaseUrl = $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(BaseUrl);
    }

    public string BaseUrl { get; }

    public void Start()
    {
        _listener.Start();
        _loop = Task.Run(AcceptLoopAsync);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Close();
        try
        {
            _loop?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Listener shutdown surfaces as an exception in the accept loop.
        }

        _cts.Dispose();
    }

    private async Task AcceptLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception) when (_cts.IsCancellationRequested)
            {
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }

            _ = Task.Run(() => Handle(context));
        }
    }

    private void Handle(HttpListenerContext context)
    {
        using var response = context.Response;
        var relative = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/").TrimStart('/');
        if (string.IsNullOrEmpty(relative))
        {
            relative = "index.html";
        }

        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));
        if (!fullPath.StartsWith(_root, StringComparison.Ordinal) || !File.Exists(fullPath))
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        var bytes = File.ReadAllBytes(fullPath);
        response.ContentType = ContentTypes.GetValueOrDefault(Path.GetExtension(fullPath), "application/octet-stream");
        response.ContentLength64 = bytes.Length;
        response.OutputStream.Write(bytes);
    }

    private static int GetFreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }
}
