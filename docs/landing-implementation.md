# WSIST — Landing Page & Amber Re-theme Implementation

Task file for autonomous execution. Work top to bottom; tasks are ordered
lowest-risk first. Commit after each task with the given message. Run
`dotnet build` after every task and `dotnet test` after T1.

---

## Context

A finished landing page design exists as a standalone HTML artifact:
**`docs/design/wsist-landing.html`** (committed alongside this file — if it is
missing, STOP and ask, do not recreate the design from this spec alone).
That file is the **source of truth** for all markup, CSS, and JS. This spec
defines how to transplant it into the Blazor Server project and re-theme the
existing app to match.

The design direction: dark editorial "grading rubric" aesthetic. Amber accent
on warm-tinted neutrals, serif display type, an interactive demo that runs the
real `PriorityCalculator` rules in client-side JS.

---

## Design tokens (canonical)

| Token | Value | Use |
|---|---|---|
| `--bg0` | `#0c0b08` | page background |
| `--bg1` | `#15110b` | elevated surfaces, cards |
| `--bg2` | `#1b1610` | higher elevation |
| `--paper` | `#f2efe6` | primary text (warm off-white, never pure white) |
| `--mist` | `#a8a294` | secondary text |
| `--faint` | `#6e6859` | tertiary text, mono labels |
| `--line` | `rgba(242,239,231,.07)` | hairline rules |
| `--line2` | `rgba(242,239,231,.14)` | stronger hairlines, card borders |
| `--accent` | `#f5b342` | THE brand color (amber); hover `#ffc75e` |
| `--accent-soft` | `rgba(245,179,66,.14)` | accent tints |
| `--mint` | `#6fe3b4` | RESERVED: "the answer" moments only (verdicts, tonight's pick) |
| `--ease` | `cubic-bezier(.23,1,.32,1)` | all UI easing (strong ease-out) |

Fonts (Google Fonts):

| Role | Family | Rules |
|---|---|---|
| Display | **Fraunces** (variable, opsz 9..144, ital) | headlines, big numbers, verdicts; `font-variation-settings:"opsz" 144` |
| UI / body | **Schibsted Grotesk** | everything else |
| Data | **Spline Sans Mono** | eyebrows, pts values, labels — genuinely tabular data ONLY, never for vibe |

Design rules (do not violate when adapting markup):

- Mint appears ONLY on answer moments. Everything interactive is amber.
- Bars/segments differentiate by amber opacity (1 / .68 / .44 / .24), not new hues.
- Hairlines + generous whitespace, no boxed-card grids, no drop-shadow soup.
- Motion: entrances ≤ 1s with `--ease`, button `:active { transform: scale(.97) }`,
  `prefers-reduced-motion` already handled in the artifact — keep those blocks.

---

## Architecture decisions (already made — implement, don't redesign)

1. **The landing page replaces `Login.razor`** at route `/login-page`.
   The auth middleware already funnels anonymous users there, and it already
   uses `EmptyLayout`. Do NOT touch `Program.cs`, middleware, or routing of `/`.
2. **Static SSR.** No `@rendermode` directive on the page. All interactivity
   is plain JS — no Blazor interop, no circuits for anonymous visitors.
3. **CSS → `WSIST.Web/wwwroot/landing.css`**, loaded globally from `App.razor`
   AFTER `app.css`. Every selector must be namespaced under `.landing`
   (details in T3) so it cannot leak into the dashboard.
4. **JS → `WSIST.Web/wwwroot/landing.js`**, loaded from `App.razor` body.
   Inline `<script>` inside a `.razor` component is a build error (RZ9992).
   The page is only ever reached via full page loads (middleware redirect,
   logout redirect), so `DOMContentLoaded` init is sufficient.
5. **All `/login` CTAs get `data-enhance-nav="false"`** so Blazor's enhanced
   navigation never intercepts the OAuth challenge endpoint (same pattern as
   existing `/logout` links).

---

## T1 — Fix the grade-pull bug in the engine

`WSIST.Engine/PriorityCalculator.cs`, `CalculateGradeScore`: the current
`_ => 0` branch gives ZERO points to subjects averaging below 3 — the opposite
of the product rule ("struggling subjects get a push"). The landing demo
already ships the corrected mapping; production must match it.

Replace the switch body:

```csharp
return average switch
{
    >= 5 => 2,
    >= 4 => 4,
    _ => 6
};
```

(The old `>= 3 => 6` branch collapses into `_ => 6`.)

Add to `WSIST.UnitTests/UnitTests.cs` (these don't need a DbContext):

```csharp
[Test]
public static void GradeScoreGivesFullPushBelowAverageOfFour()
{
    var calculator = new PriorityCalculator();
    var tests = new List<Test>
    {
        new() { Title = "T", Subject = Test.Subjects.Math, Grade = 2.5,
                DueDate = new DateOnly(2026, 01, 10) }
    };
    Assert.That(calculator.CalculateGradeScore(Test.Subjects.Math, tests), Is.EqualTo(6),
        "An average below 3 must earn the full +6 push, not 0.");
}

[Test]
public static void GradeScoreGivesSmallPushForStrongAverage()
{
    var calculator = new PriorityCalculator();
    var tests = new List<Test>
    {
        new() { Title = "T", Subject = Test.Subjects.Math, Grade = 5.5,
                DueDate = new DateOnly(2026, 01, 10) }
    };
    Assert.That(calculator.CalculateGradeScore(Test.Subjects.Math, tests), Is.EqualTo(2));
}
```

Run `dotnet csharpier .` then `dotnet test` — all green before committing.

**Commit:** `fix(engine): grade pull awards +6 below avg 4 instead of 0`

---

## T2 — Add design assets and references

1. Create `WSIST.Web/wwwroot/landing.css`: copy the ENTIRE `<style>` block
   contents from `docs/design/wsist-landing.html` (namespacing happens in T3).
2. Create `WSIST.Web/wwwroot/landing.js`: copy the ENTIRE `<script>` block
   contents verbatim. Keep the IIFE and the comment explaining the grade
   mapping. Do not convert to a module.
3. In `App.razor` `<head>`, after the existing stylesheet links, add:

```html
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Fraunces:ital,opsz,wght@0,9..144,340..640;1,9..144,340..640&family=Schibsted+Grotesk:ital,wght@0,400..900;1,400..900&family=Spline+Sans+Mono:wght@400..600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="landing.css"/>
```

4. In `App.razor` `<body>`, before the blazor.web.js script tag, add:

```html
<script src="landing.js"></script>
```

**Commit:** `feat(web): add landing design assets (fonts, landing.css, landing.js)`

---

## T3 — Namespace the landing CSS

Goal: nothing in `landing.css` may affect any page except the landing.
Apply these mechanical transformations to `landing.css`:

1. Move the `:root { ... }` variable block onto `.landing { ... }` instead.
   (The app's `app.css` also defines `--accent` etc. after T5 — values will
   match, but scoping prevents any ordering surprises.)
2. `body { ... }` styles → `.landing { ... }` (merge with the vars). Add
   `min-height:100svh` to `.landing`.
3. `body::after` (film grain) → `.landing::after` (it is `position:fixed`,
   so it still covers the viewport).
4. `html { scroll-behavior:smooth }` → keep on `html`, it's harmless globally;
   keep its reduced-motion override too.
5. The reset `*,*::before,*::after` → `.landing, .landing *, .landing *::before,
   .landing *::after` (Bootstrap's reboot is loaded globally; this scoped reset
   must win inside the landing).
6. Every remaining top-level selector gets a `.landing ` prefix
   (`.nav` → `.landing .nav`, `.hero` → `.landing .hero`, media-query contents
   included). `::selection` → `.landing ::selection`,
   `:focus-visible` → `.landing :focus-visible`.
7. Keyframes (`marquee`, `pulse`) keep their names unprefixed.

**Commit:** `style(web): namespace landing styles under .landing scope`

---

## T4 — Replace Login.razor with the landing page

Rewrite `WSIST.Web/Components/Pages/Login.razor`:

```razor
@page "/login-page"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>WSIST — What should I study today?</PageTitle>

<div class="landing">
    @* transplanted markup goes here *@
</div>
```

Inside the `.landing` div, transplant everything between `<body>` and the
`<script>` tag from `docs/design/wsist-landing.html` — nav, hero, marquee,
all sections, footer. Conversion rules:

- Strip nothing structurally; keep every class, id, `data-*`, and inline
  `style` attribute exactly. The JS targets these ids.
- Razor escaping: any literal `@` in text must become `@@` (search the
  markup; currently there are none, but verify after paste).
- Add `data-enhance-nav="false"` to every `<a href="/login">` (three: nav
  button, hero CTA, final CTA).
- Keep the HTML comments or drop them — either is fine.
- Do NOT include the `<style>` or `<script>` blocks (they live in wwwroot now).
- Do NOT add `@rendermode` — this page stays static SSR.

**Commit:** `feat(web): replace login page with landing page`

---

## T5 — Re-theme the app to the amber system

`WSIST.Web/wwwroot/app.css` — exact replacements:

| Variable / literal | Old | New |
|---|---|---|
| `--bg` | `#0f1115` | `#0c0b08` |
| `--bg-elevated` | `#15181e` | `#15110b` |
| `--text-primary` | `#e6e8eb` | `#f2efe6` |
| `--text-secondary` | `#a1a6b0` | `#a8a294` |
| `--border-subtle` | `#262a33` | `#2a251c` |
| `--accent` | `#5b8cff` | `#f5b342` |
| input backgrounds (literal, 2 occurrences) | `#11141a` | `#100d08` |
| `button.primary` / `.button-primary` text color | `#0f1115` | `#0c0b08` |

Known conflict to resolve: `.button-warning` is `#ffc107` — now nearly
identical to the amber primary, so the edit button loses meaning. Restyle it
as a ghost button instead:

```css
.button-warning {
    background: transparent;
    color: var(--text-primary);
    border: 1px solid var(--border-subtle);
}
```

Do not change layout, spacing, or any selectors beyond the table above and
this one block.

**Commit:** `style(web): re-theme app to amber design system`

---

## T6 — Privacy & Terms stubs (no dead links)

The landing footer links to `/privacy` and `/terms`. Create two minimal
static-SSR pages so they don't 404. Real GDPR content is a separate Phase 3
task — these are placeholders.

`WSIST.Web/Components/Pages/Privacy.razor` (mirror for `Terms.razor`, adjust
route/title):

```razor
@page "/privacy"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>Privacy — WSIST</PageTitle>

<div class="landing">
    <section class="section">
        <div class="container">
            <p class="eyebrow">Legal</p>
            <h2>Privacy policy</h2>
            <p class="sub">Full policy coming soon. Short version: your tests and
            grades are visible to you alone, are never sold, and you can delete
            them anytime by deleting your tests or contacting the author.</p>
            <p style="margin-top:32px"><a class="btn btn-ghost" href="/login-page">← Back</a></p>
        </div>
    </section>
</div>
```

Both routes must also bypass the auth middleware — verify they are reachable
while logged out. (They should be: the middleware only guards `/` and `/study`.)

**Commit:** `feat(web): add privacy and terms placeholder pages`

---

## Verification checklist (run before final PR)

1. `dotnet build` clean, `dotnet test` green (including the two new tests).
2. Logged out, visit `/` → redirected to `/login-page` → landing renders:
   staggered hero reveal, marquee scrolling, card bars filling.
3. **Playground math spot-checks** (move sliders, compare):

| days | volume | understanding | Ø | expected | verdict tier |
|---|---|---|---|---|---|
| 2 | High | Low | 4.2 | 8+10+10+4 = **32** | "Drop everything — this is tonight." |
| 0 | Very high | Very low | 2.0 | 10+12+12+6 = **40** | top tier |
| 35 | Very low | Very high | 5.5 | 0+2+2+2 = **6** | "Breathe. You've got time." |

4. "Continue with Google" completes OAuth and lands on the dashboard — the
   dashboard is now amber-themed, no blue remnants (search rendered CSS for
   `5b8cff`: zero hits).
5. Dashboard regression: add/edit/delete test, modal styling intact, edit
   button now ghost-styled.
6. Landing at 375px width: cards stack, sliders usable, no horizontal scroll.
7. OS reduced-motion enabled: no marquee, no tilt, content still appears.
8. `/privacy` and `/terms` load while logged out.

After deploy: **purge the Cloudflare cache** (CSS changes are cached at the
edge — known gotcha on this project).

## Out of scope — do not touch

`Program.cs`, auth middleware, OAuth config, `WsistContext`, migrations,
Railway/nixpacks config, the route of `/` or `/study`.
