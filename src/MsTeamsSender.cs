using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Utils.HttpClientCache.Abstract;
using Soenneker.Utils.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.MsTeams.Sender;

///<inheritdoc cref="IMsTeamsSender"/>
public class MsTeamsSender : IMsTeamsSender
{
    private readonly ILogger<MsTeamsSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientCache _httpClientCache;

    public MsTeamsSender(IConfiguration configuration, ILogger<MsTeamsSender> logger, IHttpClientCache httpClientCache)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientCache = httpClientCache;
    }

    public async Task<bool> SendWebhook(MsTeamsMessage message, CancellationToken cancellationToken = default)
    {
        // I wonder if we can get away without re-serializing
        string jsonContent = JsonUtil.Serialize(message.MsTeamsCard, libraryType: JsonLibraryType.Newtonsoft)!;

        if (!_configuration.GetValue<bool>("MsTeams:Enabled"))
        {
            _logger.LogWarning("MS Teams has been disabled, skipping message: {message}", jsonContent);
            return false;
        }

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var webhookUrl = _configuration.GetValueStrict<string>($"MsTeams:{message.Channel}:WebhookUrl");

        HttpResponseMessage result = await (await _httpClientCache.Get(nameof(MsTeamsSender), cancellationToken: cancellationToken).NoSync())
                                           .PostAsync(webhookUrl, content, cancellationToken)
                                           .NoSync();

        return await IsSuccessfulSend(result, cancellationToken).NoSync();
    }

    // https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using?tabs=cURL#rate-limiting-for-connectors
    private async ValueTask<bool> IsSuccessfulSend(HttpResponseMessage result, CancellationToken cancellationToken = default)
    {
        // TODO: polly retry, exp backoff

        string responseContent = await result.Content.ReadAsStringAsync(cancellationToken).NoSync();

        if (responseContent.Contains("Microsoft Teams endpoint returned HTTP error 429"))
        {
            _logger.LogError("MS Teams is rate limiting... {response}", responseContent);
            throw new Exception("MS Teams is rate limiting");
        }

        if (result.IsSuccessStatusCode && responseContent == "1")
            return true;

        _logger.LogError("Error sending MS Teams Notification ({code}): {response}", result.StatusCode, responseContent);
        return false;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return _httpClientCache.Remove(nameof(MsTeamsSender));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _httpClientCache.RemoveSync(nameof(MsTeamsSender));
    }
}