using Hangfire;
using Soenneker.Messages.MsTeams;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.MsTeams.Sender.Abstract;

/// <summary>
/// A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.
/// </summary>
public interface IMsTeamsSender : IAsyncDisposable, IDisposable
{
    [AutomaticRetry(Attempts = 0)]
    Task<bool> SendWebhook(MsTeamsMessage message, CancellationToken cancellationToken = default);
}