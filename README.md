[![](https://img.shields.io/nuget/v/soenneker.msteams.sender.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.msteams.sender/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.msteams.sender/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.msteams.sender/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.msteams.sender.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.msteams.sender/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.msteams.sender/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.msteams.sender/actions/workflows/codeql.yml)

# Soenneker.MsTeams.Sender

A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.

## Install

```bash
dotnet add package Soenneker.MsTeams.Sender
```

## Quick start

```csharp
using Soenneker.MsTeams.Sender.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddMsTeamsSenderAsSingleton();
```

Adds `IMsTeamsSender` as a singleton service.

## What you get

- `IMsTeamsSender` — A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.
- `MsTeamsSenderRegistrar` — A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IMsTeamsSender.SendMessage(message, cancellationToken)` | Sends message. | true if sends message; otherwise, false. |
| `IMsTeamsSender.SendCard(card, channel, cancellationToken)` | Sends card. | true if sends card; otherwise, false. |
| `MsTeamsSenderRegistrar.AddMsTeamsSenderAsSingleton(services)` | Adds `IMsTeamsSender` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `MsTeamsSenderRegistrar.AddMsTeamsSenderAsScoped(services)` | Adds `IMsTeamsSender` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
