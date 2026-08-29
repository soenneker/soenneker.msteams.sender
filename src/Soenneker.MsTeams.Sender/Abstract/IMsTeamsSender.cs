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
    /// <param name="message">Message content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>true if sends message; otherwise, false.</returns>
    [AutomaticRetry(Attempts = 0)]
    Task<bool> SendMessage(MsTeamsMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends card.
    /// </summary>
    /// <param name="card">Element used to host the card input.</param>
    /// <param name="channel">Delivery channel used to send the card.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>true if sends card; otherwise, false.</returns>
    Task<bool> SendCard(MsTeamsCard card, string channel, CancellationToken cancellationToken = default);
}
