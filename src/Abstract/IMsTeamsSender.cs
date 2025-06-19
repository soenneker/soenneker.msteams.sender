using Hangfire;
using Soenneker.Dtos.MsTeams.Card;
using Soenneker.Messages.MsTeams;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.MsTeams.Sender.Abstract;

/// <summary>
/// A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.
/// </summary>
public interface IMsTeamsSender
{
    [AutomaticRetry(Attempts = 0)]
    Task<bool> SendMessage(MsTeamsMessage message, CancellationToken cancellationToken = default);

    Task<bool> SendCard(MsTeamsCard card, string channel, CancellationToken cancellationToken = default);
}