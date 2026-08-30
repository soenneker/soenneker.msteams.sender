using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Dtos.MsTeams.Card;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Utils.HttpClientCache.Abstract;
using Soenneker.Utils.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.MsTeams.Sender;

public sealed class MsTeamsSender : IMsTeamsSender
{
    private readonly ILogger<MsTeamsSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientCache _httpClientCache;

    private readonly ConcurrentDictionary<string, string> _webhookUrlByChannel = new(StringComparer.Ordinal);

    public MsTeamsSender(IConfiguration configuration, ILogger<MsTeamsSender> logger, IHttpClientCache httpClientCache)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientCache = httpClientCache;
    }

    public Task<bool> SendMessage(MsTeamsMessage message, CancellationToken cancellationToken = default) =>
        SendCard(message.MsTeamsCard, message.Channel, cancellationToken);

    public async Task<bool> SendCard(MsTeamsCard card, string channel, CancellationToken cancellationToken = default)
    {
        if (!_configuration.GetValue<bool>("MsTeams:Enabled"))
        {
            _logger.LogWarning("MS Teams has been disabled, skipping message for channel {channel}", channel);
            return false;
        }

        string webhookUrl = _webhookUrlByChannel.GetOrAdd(channel, static (ch, config) => ResolveWebhookUrl(ch, config), _configuration);

        string jsonContent = JsonUtil.Serialize(card, libraryType: JsonLibraryType.Newtonsoft)!;

        using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpClient client = await _httpClientCache.Get(nameof(MsTeamsSender), cancellationToken: cancellationToken)
                                                  .NoSync();

        using HttpResponseMessage response = await client.PostAsync(webhookUrl, content, cancellationToken)
                                                         .NoSync();

        return await IsSuccessfulSend(response)
            .NoSync();
    }

    private ValueTask<bool> IsSuccessfulSend(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return ValueTask.FromResult(true);

        if (response.StatusCode == (HttpStatusCode)429)
        {
            _logger.LogWarning("MS Teams rejected the notification because the webhook is rate limited (429)");
            return ValueTask.FromResult(false);
        }

        _logger.LogError("MS Teams notification failed with status code {StatusCode}", response.StatusCode);
        return ValueTask.FromResult(false);
    }

    private static string ResolveWebhookUrl(string channel, IConfiguration configuration)
    {
        if (channel.Contains(':', StringComparison.Ordinal))
            throw new InvalidOperationException("MS Teams channel names cannot contain configuration path separators.");

        string value = configuration.GetValueStrict<string>($"MsTeams:{channel}:WebhookUrl");

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException($"The MS Teams webhook configured for channel '{channel}' must be an absolute HTTPS URL.");

        return uri.AbsoluteUri;
    }
}
