using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Audit;

public class ResponseBufferingTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    [Test]
    [Arguments(200, true)]
    [Arguments(429, false)]
    [Arguments(500, false)]
    public async Task TeamsSendDoesNotBufferUnusedResponseBody(int status, bool expected)
    {
        var body = new UnreadableContent();
        using var http = new System.Net.Http.HttpClient(new ResponseHandler(body, status));
        var cache = System.Reflection.DispatchProxy.Create<Soenneker.Utils.HttpClientCache.Abstract.IHttpClientCache, HttpCacheProxy>();
        ((HttpCacheProxy)(object)cache).Client = http;
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MsTeams:Enabled"] = "true", ["MsTeams:audit:WebhookUrl"] = "https://audit.invalid/webhook"
        }).Build();
        var sender = new Soenneker.MsTeams.Sender.MsTeamsSender(config, NullLogger<Soenneker.MsTeams.Sender.MsTeamsSender>.Instance, cache);
        bool result = await sender.SendCard(new Soenneker.Dtos.MsTeams.Card.MsTeamsCard(), "audit");
        Check(result == expected && body.Disposed, "HTTP status handling or response disposal changed");
    }
    public class HttpCacheProxy : System.Reflection.DispatchProxy
    {
        public System.Net.Http.HttpClient Client = null!;
        protected override object? Invoke(System.Reflection.MethodInfo? targetMethod, object?[]? args) => ValueTask.FromResult(Client);
    }
    private sealed class ResponseHandler(UnreadableContent body, int status) : System.Net.Http.HttpMessageHandler
    {
        protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Check(request.Method == System.Net.Http.HttpMethod.Post && request.Content!.Headers.ContentType!.MediaType == "application/json", "HTTP request changed");
            return Task.FromResult(new System.Net.Http.HttpResponseMessage((System.Net.HttpStatusCode)status) { Content = body });
        }
    }
    private sealed class UnreadableContent : System.Net.Http.HttpContent
    {
        public bool Disposed;
        protected override Task SerializeToStreamAsync(System.IO.Stream stream, System.Net.TransportContext? context) =>
            throw new InvalidOperationException("Unused response body was buffered");
        protected override bool TryComputeLength(out long length) { length = 1_000_000; return true; }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
