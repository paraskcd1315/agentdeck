---
name: ad-panel
description: Render a native panel in AgentDeck. Use when the user asks to show status, a summary, a checklist, results, or any structured information as a panel in the AgentDeck window rather than as terminal text.
---

# ad-panel

Write a JSON file and AgentDeck renders it as a native panel. There is no API to call and no command to
run — the file **is** the panel.

## Where

`~/.agentdeck/workspaces/<workspaceId>/panels/<id>.json`

`<workspaceId>` is printed by AgentDeck when the workspace opens, and `<id>` is any short kebab-case name
you choose. Writing to the same `<id>` again replaces that panel; a new `<id>` adds one.

**Write atomically**: write to `<id>.json.tmp` in the same directory, then rename it over `<id>.json`.
A panel read halfway through a write renders as garbage.

## Shape

```json
{
  "schema": "panel/v1",
  "title": "Test results",
  "blocks": []
}
```

`"schema"` must be exactly `panel/v1`. Anything else renders as an "unsupported version" card.

## Blocks

| Block | Shape | Use for |
|---|---|---|
| `markdown` | `{ "type": "markdown", "text": "..." }` | prose, lists, code fences |
| `keyvalue` | `{ "type": "keyvalue", "rows": [{ "key": "...", "value": "..." }] }` | facts, counts, versions |
| `status` | `{ "type": "status", "state": "idle\|working\|waiting\|failed", "text": "..." }` | one headline state |
| `actions` | `{ "type": "actions", "buttons": [{ "label": "...", "prompt": "...", "target": "live" }] }` | buttons that send a prompt back |

An `actions` button injects its `prompt` into the **live** session when `target` is `live` (the default),
which is the same as the user typing it. `target: "new"` is reserved for spawning a fresh session.

## Rules

- Compose the existing four blocks rather than inventing a fifth. Every block type is a permanent
  compatibility obligation across both platform shells.
- One panel per subject. Don't accumulate unrelated information in one file.
- Keep `title` short — it is a header, not a sentence.
- Never put secrets, tokens, or absolute paths containing usernames in a panel.

## Example

```json
{
  "schema": "panel/v1",
  "title": "Spike 0",
  "blocks": [
    { "type": "status", "state": "working", "text": "6 of 9 steps done" },
    { "type": "keyvalue", "rows": [
      { "key": "Daemon", "value": "listening" },
      { "key": "Shells verified", "value": "PowerShell, Git Bash, WSL" }
    ]},
    { "type": "markdown", "text": "Channel  and  are live. Panels render from disk." },
    { "type": "actions", "buttons": [
      { "label": "Run the smoke test", "prompt": "Run the PTY smoke test for all three shells" }
    ]}
  ]
}
```
