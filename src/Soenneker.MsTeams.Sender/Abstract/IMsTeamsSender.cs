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
    /// <summary>
    /// Sends message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [AutomaticRetry(Attempts = 0)]
    Task<bool> SendMessage(MsTeamsMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends card.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    Task<bool> SendCard(MsTeamsCard card, string channel, CancellationToken cancellationToken = default);
}