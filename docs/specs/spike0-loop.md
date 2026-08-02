# Spike 0 — The Loop Spike

**Status**: in progress · **Jira**: ADECK-2 · **Started**: 2026-08-02

Spike 0 is blocking. Nothing else in the roadmap starts until this loop closes.

## 1. What this proves

That the whole product premise works end to end in its smallest honest form: a native shell drives a
real PTY through a Rust daemon, the agent running in that PTY can reach back into the UI through two
independent channels, and the UI can push a prompt back into the same live session without a second
control path.

Six claims, each separately falsifiable:

| # | Claim |
|---|---|
| 1 | A WinUI 3 window hosts a PTY running interactive `claude`, driven by the Rust daemon over JSON-RPC |
| 2 | A `PostToolUse` hook fires into the app over a local socket while that session runs |
| 3 | A hand-written `panel.json` renders as a native panel on the AgentDeck tokens |
| 4 | A one-line skill makes the agent author that JSON itself |
| 5 | A button injects a prompt into the running session |
| 6 | PowerShell, Git Bash and WSL each work in that PTY — ConPTY is not one uniform path |

## 2. Non-goals

Stated so the spike is not judged against them, and so nothing here is mistaken for a design decision
that survives into phase 1.

- **No VT parsing.** The daemon pumps raw bytes; the shell renders them into a naive scrolling text
  view. No cell grid, no alt-screen, no scrollback model, no `alacritty_terminal`.
- **Powerlevel10k will not render correctly, and that is expected.** P10k emits truecolor SGR and
  powerline glyphs that a passthrough view cannot place. Step 6 records what it actually looks like as
  the phase-1 baseline. Correct P10k rendering is a **phase 1 exit criterion**, not a spike criterion.
- No tabs, splits, profiles or `autoStart`. One PTY at a time.
- No editing, no LSP, no git, no diff, no session archive.
- No schema validation beyond "is it the version we know" — the invalid-panel card is phase 2.
- Not packaged, not signed, not installed. `dotnet run` and `cargo run`.

## 3. Topology

```
┌──────────────────────┐        JSON-RPC 2.0 (NDJSON)        ┌─────────────────────┐
│  WinUI 3 shell       │◄───────────────────────────────────►│  agentdeckd (Rust)  │
│  windows/            │      \\.\pipe\agentdeck              │  core/agentdeckd    │
│  text view · panel   │                                      │                     │
│  · inject button     │                                      │  ConPTY ─ claude    │
└──────────────────────┘                                      │  watcher ─ panels   │
                                                              └─────────────────────┘
                                                                        ▲
                              PostToolUse ─► agentdeck-hook ─────────────┘
                                             (same pipe, one-shot notify)
```

Three processes. The daemon is the only one that owns state. The hook is a short-lived one-shot client
that connects, writes one notification and exits — which is why it reuses the same transport rather
than earning a second one.

## 4. IPC

**Transport**: Windows named pipe `\\.\pipe\agentdeck`, message-mode, multiple client instances.
A unix socket at `$XDG_RUNTIME_DIR/agentdeck.sock` is the macOS equivalent; out of scope here but the
protocol is identical, which is the point of choosing a byte-stream transport.

**Framing**: newline-delimited JSON. One JSON-RPC 2.0 object per line, UTF-8, `\n` terminated. Chosen
over LSP-style `Content-Length` headers because it is directly readable with any pipe client, which
matters for a spike whose whole job is to be diagnosable.

**Binary payloads are base64.** PTY output is a raw byte stream and a UTF-8 codepoint can straddle two
reads, so bytes never cross the wire as a JSON string. The shell accumulates and decodes incrementally.

### Methods — shell → daemon

| Method | Params | Result |
|---|---|---|
| `pty.spawn` | `{ program, args[], cwd?, env?, cols, rows }` | `{ ptyId }` |
| `pty.write` | `{ ptyId, dataB64 }` | `{}` |
| `pty.resize` | `{ ptyId, cols, rows }` | `{}` |
| `pty.kill` | `{ ptyId }` | `{}` |
| `panel.list` | `{ workspaceId }` | `{ panels: [{ id, path }] }` |
| `panel.read` | `{ workspaceId, id }` | `{ panel }` |

### Notifications — daemon → shell

| Notification | Params |
|---|---|
| `pty.data` | `{ ptyId, dataB64 }` |
| `pty.exit` | `{ ptyId, code }` |
| `hook.event` | `{ hook, tool?, payload, receivedAt }` |
| `panel.changed` | `{ workspaceId, id, path }` |

### Notification — hook → daemon

`hook.event` with the same shape. The daemon fans it out to every connected shell. A hook that finds no
daemon listening exits 0 silently — the agent must never fail because the GUI is closed.

## 5. Channel ① — the hook

`agentdeck-hook` is a second binary in the `core/` cargo workspace. It reads the Claude Code hook
payload from stdin, wraps it in a `hook.event` notification, writes one line to the pipe and exits.
Sharing a workspace with the daemon means the pipe client code is exercised by two independent callers,
which is free coverage on the transport.

Registered in the spike workspace's `.claude/settings.json`:

```json
{ "hooks": { "PostToolUse": [ { "matcher": "", "hooks": [
  { "type": "command", "command": "<repo>/core/target/debug/agentdeck-hook.exe" } ] } ] } }
```

**Acceptance**: editing a file through the agent in the hosted terminal makes an entry appear in the
shell's event list, live, without the shell polling anything.

## 6. Channel ④ — the panel

**State directory**, per ProjectContext §State Directory:

```
~/.agentdeck/
  workspaces/<workspaceId>/
    panels/*.json
```

`workspaceId` is the first 16 hex characters of the SHA-256 of the workspace's absolute path,
lowercased and with separators normalised. Per-user, never per-repo.

**Schema**, `schemas/panel.v1.json`. Every file carries `"schema": "panel/v1"`; anything else renders as
an "unsupported version" card rather than as garbage.

```jsonc
{
  "schema": "panel/v1",
  "title": "string",
  "blocks": [ /* markdown | keyvalue | status | actions */ ]
}
```

The spike implements four of the nine block types — the rest are phase 2:

| Block | Shape |
|---|---|
| `markdown` | `{ type, text }` |
| `keyvalue` | `{ type, rows: [{ key, value }] }` |
| `status` | `{ type, state: "idle"\|"working"\|"waiting"\|"failed", text }` |
| `actions` | `{ type, buttons: [{ label, prompt, target: "live"\|"new" }] }` |

**Writes are atomic** — temp file in the same directory, then rename. Non-negotiable from day one: a
watcher that reads a half-written file is the failure mode this prevents, and it is unfixable later
without rewriting every producer.

**Rendering** uses `design/ad-tokens.css` from the ClaudeContext project folder, translated to
`AgentDeck.Tokens.xaml`. Glass is the substrate; `status` maps to `--state-*`.

**The skill** (`skills/ad-panel/SKILL.md`) is one instruction: write a `panel/v1` file to the workspace's
panel directory. Claim 4 is proven when the agent, told only that, produces a file the renderer accepts.

## 7. Channel in — injection

An `actions` button sends `pty.write` with its `prompt` plus `\r` to the PTY hosting `claude`. That is
the entire mechanism, and it is the only way the UI ever influences the agent. `target: "new"` is
specified but not implemented in the spike — conflating "inject into the live session" with "spawn a
fresh one" is the obvious bug, so the field exists from the start to keep them distinct.

## 8. Rust module layout

`core/` is a cargo workspace. Taxonomy from the first file, one type per file, per ProjectContext
§Working Agreement.

```
core/
  Cargo.toml                  workspace manifest
  agentdeckd/src/
    main.rs                   wiring and dispatch only
    constants.rs
    config/paths.rs           StateDirs
    pty/conpty.rs             ConPty          (the unsafe Win32 FFI, isolated here)
    pty/session.rs            PtySession
    pty/session_id.rs         PtyId
    pty/size.rs               PtySize
    pty/error.rs              PtyError
    rpc/server.rs             RpcServer
    rpc/connection.rs         Connection
    rpc/request.rs            Request
    rpc/response.rs           Response
    rpc/notification.rs       Notification
    rpc/method.rs             Method
    rpc/error.rs              RpcError
    panel/panel.rs            Panel
    panel/block.rs            Block
    panel/watcher.rs          PanelWatcher
  agentdeck-hook/src/main.rs
```

All `unsafe` lives in `pty/conpty.rs`. Nothing else in the crate may contain it — one file to audit, and
the soundness argument stays in one place.

## 9. Acceptance criteria

The spike closes when all six hold in one sitting, in one window:

1. `cargo run -p agentdeckd` starts; `dotnet run` in `windows/` connects to the pipe.
2. The shell spawns PowerShell, Git Bash and WSL in turn. Each accepts input, echoes output and exits
   cleanly. Behavioural differences are **recorded**, not smoothed over.
3. `claude` runs interactively in that PTY and can hold a conversation.
4. A `PostToolUse` fired by that agent appears in the shell's event list.
5. A hand-written `panel.json` renders as a native panel; a skill-authored one renders identically.
6. Clicking an `actions` button puts its prompt into the live session and the agent responds to it.

Anything that fails is written down rather than worked around. A spike that reports a real obstacle has
succeeded; a spike that hides one has not.

## 10. Risks this spike is actually testing

- **Risk 3 — terminal emulation is a swamp.** Not resolved here, but the ConPTY boundary is exercised
  and the cost of the real emulator becomes estimable.
- **Risk 4 — Git Bash / MSYS2 and WSL are not the same PTY path.** Directly tested in step 6. MSYS2
  carries its own pty emulation, and Git Bash puts children in the caller's job object.
- **Risk 1 — the agent is a UI author and agents produce invalid output.** First contact in step 8: the
  skill-authored file either matches the schema or it does not.
