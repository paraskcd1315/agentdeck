# Theming

AgentDeck reads `~/.agentdeck/config.json` at startup. Every key is optional; anything absent falls back
to the design-system value. See [`config.example.json`](config.example.json) for a working file.

Restart the app after editing — live reload is not implemented yet.

## `theme`

| Key | Default | Effect |
|---|---|---|
| `mode` | `dark` | `dark` or `light` |
| `backgroundGradientFrom` | `#252932` | Top-left stop of the app background wash |
| `backgroundGradientTo` | `#0E0F12` | Bottom-right stop |
| `backgroundGradientOpacity` | `0.75` | How strongly the wash sits over the acrylic. Lower shows more desktop through the window |

The background is a **gradient, not a flat colour**, on purpose: glass over a flat wash reads as a grey
rectangle, so the substrate has to give the blur something to vary against.

## `theme.terminal`

| Key | Default | Effect |
|---|---|---|
| `opacity` | `0.92` | Canvas opacity |
| `blur` | `0` | Gaussian blur radius. **`0` keeps the canvas flat**; any value above zero switches it to the same glass brush the panels use |
| `saturation` | `1.15` | Saturation applied with the blur |
| `background` | `#0B0C0F` | Canvas colour |
| `foreground` | `#D6D9E0` | Default text colour |

**Why blur defaults to off.** The design system makes the terminal the one non-glass surface: text over a
moving blur is a readability failure, and this is a surface read for hours. That remains the default and
the reason still holds — but it is a default, not a prohibition, so `blur` is yours to turn on.

## Not themeable

The spacing, radius and elevation scales, and every palette that encodes meaning — diff add/remove, log
levels, agent state. Letting those be recoloured is how a design system stops meaning anything.
