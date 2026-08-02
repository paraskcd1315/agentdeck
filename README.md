# AgentDeck

A native desktop shell built around the Claude Code CLI. A real terminal — any shell, any binary — with
`claude` running in it, surrounded by a UI that reacts: diff viewer, file explorer, step tracker,
session browser, Jira panels.

No Chromium as the UI runtime. No Node in the shipped app. No reimplementation of the agent loop — the
CLI is the engine, unmodified, in a PTY.

## Status

**Spike 0 — the loop spike.** Nothing else in the roadmap starts until this returns something real.
See [`docs/specs/spike0-loop.md`](docs/specs/spike0-loop.md).

## Layout

| Path | Contents |
|---|---|
| `core/` | Rust daemon — PTY, terminal state, git, watchers, transcript parsing, JSON-RPC server |
| `windows/` | WinUI 3 / C# / .NET 10 shell |
| `macos/` | SwiftUI shell — post-MVP |
| `schemas/` | JSON Schema for every panel version; the contract between skills and both shells |
| `skills/` | The skill pack that authors panel JSON |
| `docs/specs/` | Spec-driven-development specs |

## Architecture

One channel in, five out. **In** is the PTY — UI buttons compose prompts and inject them into the live
session, never a second control path. **Out** is ① Claude Code hooks, ② the transcript tail, ③ filesystem
and git watchers, ④ the state directory under `~/.agentdeck/` where skills write versioned JSON that the
app renders as native panels, and ⑤ the Chrome DevTools Protocol from an optional on-demand dev browser.

The shell owns no state that survives a window close. The daemon owns everything durable.

## Building

Requires the .NET 10 SDK, the Rust MSVC toolchain, and VS Build Tools with the C++ workload.

```
cd core    && cargo build
cd windows && dotnet build
```
