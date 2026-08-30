# Soenneker.MsTeams.Sender
[![](https://img.shields.io/nuget/v/soenneker.msteams.sender.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.msteams.sender/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.msteams.sender/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.msteams.sender/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.msteams.sender.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.msteams.sender/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.msteams.sender/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.msteams.sender/actions/workflows/codeql.yml)

Routes Adaptive Card payloads to named Microsoft Teams HTTPS webhooks.

## Installation

```bash
dotnet add package Soenneker.MsTeams.Sender
```

## Configuration

Configure the feature switch and one webhook per logical channel:

```json
{
  "MsTeams": {
    "Enabled": true,
    "EngineeringAlerts": {
      "WebhookUrl": "https://example.webhook.office.com/webhookb2/..."
    }
  }
}
```

The webhook must be an absolute HTTPS URL. Treat it as a credential: keep it in a secret provider, do not commit it to source control, and do not include it in logs. Channel names cannot contain `:`.

## Registration

Choose the sender lifetime that matches how webhook configuration is refreshed:

```csharp
using Soenneker.MsTeams.Sender.Registrars;

builder.Services.AddMsTeamsSenderAsSingleton();
// or: builder.Services.AddMsTeamsSenderAsScoped();
```

Both registrations keep the underlying HTTP client cache singleton. A scoped sender can be discarded while the shared client remains available. Each sender caches the first webhook URL resolved for a channel, so a singleton sender requires an application restart to pick up a changed URL; a scoped sender resolves it again in a new scope. The `MsTeams:Enabled` switch is read on every send.

## Send a card

```csharp
using Soenneker.Dtos.MsTeams.Card;
using Soenneker.MsTeams.Sender.Abstract;

public sealed class DeploymentNotifier(IMsTeamsSender teams)
{
    public Task<bool> Notify(MsTeamsCard card, CancellationToken cancellationToken) =>
        teams.SendCard(card, "EngineeringAlerts", cancellationToken);
}
```

`SendMessage` accepts a `MsTeamsMessage` and reads its `MsTeamsCard` and `Channel` fields. `SendCard` sends the card directly.

The methods return `true` only for a successful HTTP status. Disabled sending, rate limiting (`429`), and other non-success responses return `false` and are logged without response bodies. Configuration errors, serialization failures, transport errors, and cancellation are surfaced as exceptions.

`SendMessage` disables Hangfire automatic retries through its interface attribute. If delivery must be retried, define an explicit policy that accounts for duplicate webhook delivery and the `false` result.
