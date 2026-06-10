# WSIST — Full Redesign Task File

## Context for the model

WSIST is a Blazor Server app styled with plain CSS in `app.css`. There is no Tailwind, no React, no npm. Launch UI is used as **visual reference only** — translate its design language into hand-crafted CSS. Do not attempt to install any npm packages or import React components.

The goal: dark, modern, premium-feeling app. Amber accent (`#f59e0b`) instead of the generic blue. Plus Jakarta Sans instead of Inter. Launch UI's surface and component aesthetics — subtle borders, sticky blur nav, ambient glows, proper hover states — all implemented as vanilla CSS.

Branch: `feat/redesign`

Run `dotnet build` before each commit. Do not break any functionality.

---

## Commit 1 — Design tokens, typography, global resets

### Step 1 — Load Plus Jakarta Sans in `App.razor`

Inside `<head>`, after the existing `<base href="/">` tag, add:

```razor
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous"/>
<link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@300;400;500;600;700;800&display=swap" rel="stylesheet"/>
```

### Step 2 — Replace the entire `:root` block and global base in `app.css`

Replace everything from the top of `app.css` up to and including the existing `.cards {}` rule with:

```css
:root {
    --bg: #09090b;
    --bg-elevated: #111113;
    --bg-subtle: #18181b;
    --bg-hover: #1c1c1f;
    --text-primary: #fafafa;
    --text-secondary: #a1a1aa;
    --text-muted: #52525b;
    --border: #27272a;
    --border-hover: #3f3f46;
    --accent: #f59e0b;
    --accent-hover: #fbbf24;
    --accent-dim: rgba(245, 158, 11, 0.12);
    --accent-border: rgba(245, 158, 11, 0.25);
    --accent-glow: rgba(245, 158, 11, 0.08);
    --radius: 10px;
    --radius-lg: 14px;
    --radius-sm: 6px;
    --radius-xs: 4px;
    --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.5);
    --shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    --shadow-lg: 0 20px 60px rgba(0, 0, 0, 0.5);
    --transition: 150ms cubic-bezier(0.4, 0, 0.2, 1);
    --transition-slow: 300ms cubic-bezier(0.4, 0, 0.2, 1);
}

*, *::before, *::after {
    box-sizing: border-box;
}

html {
    scroll-behavior: smooth;
}

html, body {
    background: var(--bg);
    color: var(--text-primary);
    font-family: 'Plus Jakarta Sans', system-ui, -apple-system, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    font-size: 15px;
    line-height: 1.5;
    margin: 0;
    padding: 0;
}

/* Subtle noise grain overlay for depth */
body::after {
    content: '';
    position: fixed;
    inset: 0;
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)'/%3E%3C/svg%3E");
    opacity: 0.025;
    pointer-events: none;
    z-index: 9999;
}

h1, h2, h3, h4, h5, h6 {
    color: var(--text-primary);
    font-weight: 700;
    letter-spacing: -0.02em;
    line-height: 1.2;
    margin: 0;
    text-wrap: balance;
}

p {
    color: var(--text-secondary);
    margin: 0;
    line-height: 1.6;
}

a {
    color: inherit;
    text-decoration: none;
}

input, select, textarea, button {
    font-family: inherit;
}

.cards {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: 24px;
}
```

**Commit:**
```
design: update CSS variables, load Plus Jakarta Sans, add global resets
```

---

## Commit 2 — Navigation, buttons, forms, modals

### Replace every existing rule in `app.css` that matches the selectors below

Replace the `.top-row` rule with:

```css
.page {
    display: flex;
    flex-direction: column;
    min-height: 100dvh;
}

main {
    flex: 1;
}

.top-row {
    position: sticky;
    top: 0;
    z-index: 50;
    background: rgba(9, 9, 11, 0.85);
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    border-bottom: 1px solid var(--border);
    height: 56px;
    display: flex;
    align-items: center;
    padding: 0 1.5rem;
    gap: 10px;
}

.title {
    margin-right: auto;
    font-size: 17px;
    font-weight: 700;
    letter-spacing: -0.02em;
    color: var(--text-primary);
}

.nav-brand {
    display: flex;
    align-items: center;
    gap: 9px;
    margin-right: auto;
}

.nav-brand span {
    font-size: 17px;
    font-weight: 700;
    letter-spacing: -0.02em;
    color: var(--text-primary);
}
```

Replace the `.button-primary` rule and all `.button-*` variants with:

```css
.button-primary {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    background: var(--bg-subtle);
    color: var(--text-primary);
    border: 1px solid var(--border);
    border-radius: var(--radius-sm);
    padding: 7px 14px;
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    white-space: nowrap;
    text-decoration: none;
    transition: background var(--transition), border-color var(--transition), transform var(--transition);
    line-height: 1;
}

.button-primary:hover {
    background: var(--bg-hover);
    border-color: var(--border-hover);
}

.button-primary:active {
    transform: scale(0.97);
}

/* Accent (CTA) button */
.button-primary.button-accent,
a.button-primary[href="/login"],
a.button-primary[href="/study"] {
    background: var(--accent);
    color: #09090b;
    border-color: transparent;
    font-weight: 600;
}

.button-primary.button-accent:hover,
a.button-primary[href="/login"]:hover,
a.button-primary[href="/study"]:hover {
    background: var(--accent-hover);
    border-color: transparent;
}

.button-warning {
    background: rgba(251, 191, 36, 0.15);
    border-color: rgba(251, 191, 36, 0.3);
    color: #fbbf24;
}

.button-warning:hover {
    background: rgba(251, 191, 36, 0.22);
}

.button-danger {
    background: rgba(239, 68, 68, 0.12);
    border-color: rgba(239, 68, 68, 0.25);
    color: #f87171;
}

.button-danger:hover {
    background: rgba(239, 68, 68, 0.2);
}
```

Replace all form input styles (`.wsist-modal input`, `.wsist-modal select`, etc.) with:

```css
.wsist-modal input,
.wsist-modal select,
.wsist-modal textarea,
.settings-input,
.study-input-row input {
    width: 100%;
    background: var(--bg);
    color: var(--text-primary);
    border: 1px solid var(--border);
    border-radius: var(--radius-sm);
    padding: 9px 12px;
    font-size: 14px;
    font-family: inherit;
    line-height: 1.5;
    transition: border-color var(--transition), box-shadow var(--transition);
    outline: none;
}

.wsist-modal input:focus,
.wsist-modal select:focus,
.wsist-modal textarea:focus,
.settings-input:focus,
.study-input-row input:focus {
    border-color: var(--accent);
    box-shadow: 0 0 0 3px var(--accent-dim);
}

.wsist-modal input::placeholder,
.settings-input::placeholder {
    color: var(--text-muted);
}

.wsist-modal select {
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23a1a1aa' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 12px center;
    padding-right: 36px;
}

/* Form layout within modal */
.form-group {
    margin-bottom: 18px;
    text-align: left;
}

.form-group label,
.wsist-modal label {
    display: block;
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary);
    margin-bottom: 6px;
    letter-spacing: 0.01em;
}
```

Replace the modal styles:

```css
.wsist-modal {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.65);
    backdrop-filter: blur(6px);
    -webkit-backdrop-filter: blur(6px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    padding: 24px;
}

.wsist-modal-content {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    width: 420px;
    max-width: 100%;
    padding: 28px;
    box-shadow: var(--shadow-lg);
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.wsist-modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.wsist-modal-title {
    font-size: 17px;
    font-weight: 600;
    color: var(--text-primary);
    letter-spacing: -0.01em;
}

.wsist-modal-close {
    background: none;
    border: none;
    color: var(--text-muted);
    font-size: 18px;
    cursor: pointer;
    padding: 4px;
    border-radius: var(--radius-xs);
    transition: color var(--transition);
    line-height: 1;
}

.wsist-modal-close:hover {
    color: var(--text-primary);
}
```

Replace the table styles:

```css
.table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
}

.table thead th {
    text-align: left;
    padding: 8px 14px;
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    border-bottom: 1px solid var(--border);
    background: transparent;
}

.table tbody td {
    padding: 13px 14px;
    border-bottom: 1px solid var(--border);
    color: var(--text-primary);
    background: transparent;
    vertical-align: middle;
}

.table tbody tr:last-child td {
    border-bottom: none;
}

.table tbody tr {
    transition: background var(--transition);
}

.table tbody tr:hover td {
    background: var(--bg-subtle);
}
```

**Commit:**
```
design: redesign nav, buttons, forms, modals, table
```

---

## Commit 3 — Home page

### Update `Home.razor`

Replace the content section structure. The goal is:

1. Page title section uses the new `.top` layout
2. Recommendation widget looks better
3. Table wrapped in a container card
4. Buttons are properly styled

Replace the entire contents of `Home.razor` with:

```razor
@page "/"
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using WSIST.Engine
<PageTitle>WSIST</PageTitle>

<div class="page">
    <div class="top-row">
        <div class="title nav-brand">
            <img src="/logo.svg" width="26" height="26" alt="WSIST logo"/>
            <span>WSIST</span>
        </div>
        <button class="button-primary" @onclick="Refresh">↻</button>
        <a href="/settings" class="button-primary" data-enhance-nav="false">Settings</a>
        <a href="/study" class="button-primary" data-enhance-nav="false">Study →</a>
        <a href="/logout" class="button-primary button-danger" data-enhance-nav="false">Logout</a>
    </div>

    <div class="content px-4">

        <div class="home-header">
            <div>
                <h1 class="home-title">Your tests</h1>
                <p class="home-subtitle">Upcoming exams and assessments.</p>
            </div>
            <div class="home-header-actions">
                <button class="button-primary" @onclick="TogglePastTests">
                    @(showPastTests ? "Hide past" : "Show past")
                </button>
                <button class="button-primary button-accent" @onclick="OpenAddTestModal">+ Add test</button>
            </div>
        </div>

        @if (topRecommendation is not null)
        {
            var score = calculator.CalculateTotalScore(topRecommendation, allTests);
            var days = topRecommendation.DueDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
            <div class="recommendation-widget">
                <div class="recommendation-widget-inner">
                    <div class="recommendation-widget-label">Study today</div>
                    <div class="recommendation-widget-body">
                        <div>
                            <div class="recommendation-widget-title">@topRecommendation.Title</div>
                            <div class="recommendation-widget-meta">
                                @(subjects.FirstOrDefault(s => s.Id == topRecommendation.Subject)?.Name ?? "Unknown") · in @days day@(days == 1 ? "" : "s") · @score/40 pts
                            </div>
                        </div>
                        <a href="/study" class="button-primary" data-enhance-nav="false" style="flex-shrink:0">Full plan →</a>
                    </div>
                    <div class="study-card-bar-bg">
                        <div class="study-card-bar" style="width: @(score / 40.0 * 100)%"></div>
                    </div>
                </div>
            </div>
        }

        <div class="table-card">
            @if (!tests.Any())
            {
                <div class="empty-state">
                    <p class="empty-state-icon">📋</p>
                    <p class="empty-state-title">No upcoming tests</p>
                    <p class="empty-state-sub">Add your first test to get started.</p>
                </div>
            }
            else
            {
                <table class="table">
                    <thead>
                        <tr>
                            <th>Title</th>
                            <th>Subject</th>
                            <th>Date</th>
                            <th>Volume</th>
                            <th>Understanding</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var test in tests)
                        {
                            <tr>
                                <td class="td-title">@test.Title</td>
                                <td class="td-muted">@(subjects.FirstOrDefault(s => s.Id == test.Subject)?.Name ?? "Unknown")</td>
                                <td class="td-muted">@test.DueDate.ToShortDateString()</td>
                                <td class="td-muted">@Test.VolumeHelper(test.Volume)</td>
                                <td class="td-muted">@Test.UnderstandingHelper(test.Understanding)</td>
                                <td class="td-actions">
                                    <button class="button-primary" @onclick="() => OpenEditTestModal(test)">Edit</button>
                                    <button class="button-primary button-danger" @onclick="() => DeleteTest(test.Id)">Delete</button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            }
        </div>

        @{
            var subjectAverages = GetSubjectAverages();
        }
        @if (subjectAverages.Any())
        {
            <div class="grade-overview">
                <h2 class="grade-overview-title">Grades</h2>
                <div class="grade-overview-cards">
                    @foreach (var (subject, avg) in subjectAverages.OrderBy(x => x.Value))
                    {
                        <div class="grade-overview-card">
                            <span class="grade-overview-subject">@(subjects.FirstOrDefault(s => s.Id == subject)?.Name ?? subject.ToString())</span>
                            <span class="grade-overview-value @GetGradeClass(avg)">@avg.ToString("F1")</span>
                        </div>
                    }
                </div>
            </div>
        }

        @if (showModal && temporaryTest is not null)
        {
            <div class="wsist-modal">
                <div class="wsist-modal-content">
                    <div class="wsist-modal-header">
                        <h2 class="wsist-modal-title">@(Mode == Modes.AddTest ? "Add test" : "Edit test")</h2>
                        <button class="wsist-modal-close" @onclick="CloseModal">×</button>
                    </div>
                    <form @onsubmit="ModalSubmit">
                        <div class="form-group">
                            <label>Title</label>
                            <input type="text" @bind="@temporaryTest.Title" placeholder="e.g. Maths Exam"/>
                        </div>
                        <div class="form-group">
                            <label>Subject</label>
                            <select @bind="temporaryTest.Subject">
                                @foreach (var subject in subjects)
                                {
                                    <option value="@subject.Id">@subject.Name</option>
                                }
                            </select>
                        </div>
                        <div class="form-group">
                            <label>Due date</label>
                            <input type="date" @bind="@temporaryTest.DueDate" @bind:event="oninput"/>
                        </div>
                        <div class="form-group">
                            <label>Volume</label>
                            <select @bind="temporaryTest.Volume">
                                @foreach (var v in Enum.GetValues<Test.TestVolume>())
                                {
                                    <option value="@v">@Test.VolumeHelper(v)</option>
                                }
                            </select>
                        </div>
                        <div class="form-group">
                            <label>Understanding</label>
                            <select @bind="temporaryTest.Understanding">
                                @foreach (var u in Enum.GetValues<Test.PersonalUnderstanding>())
                                {
                                    <option value="@u">@Test.UnderstandingHelper(u)</option>
                                }
                            </select>
                        </div>
                        <div class="form-group">
                            <label>Grade @(temporaryTest.DueDate > DateOnly.FromDateTime(DateTime.Today) ? "(available after test date)" : "")</label>
                            <input type="number" min="1" max="6" step="0.1"
                                   @bind="temporaryTest.Grade"
                                   disabled="@(temporaryTest.DueDate > DateOnly.FromDateTime(DateTime.Today))"/>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="button-primary" @onclick="CloseModal">Cancel</button>
                            <button type="submit" class="button-primary button-accent">
                                @(Mode == Modes.AddTest ? "Add test" : "Save changes")
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        }
    </div>
</div>
```

### Add to `app.css`:

```css
/* ── Home Page ── */

.home-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    padding: 1.5rem 0 1.25rem;
    gap: 16px;
    flex-wrap: wrap;
}

.home-title {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.02em;
}

.home-subtitle {
    font-size: 13px;
    color: var(--text-muted);
    margin-top: 3px;
}

.home-header-actions {
    display: flex;
    gap: 8px;
    align-items: center;
    flex-shrink: 0;
}

.table-card {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    overflow: hidden;
    margin-bottom: 1.5rem;
}

.td-title {
    font-weight: 500;
    color: var(--text-primary);
}

.td-muted {
    color: var(--text-secondary);
}

.td-actions {
    display: flex;
    gap: 6px;
    justify-content: flex-end;
}

.empty-state {
    padding: 3rem 2rem;
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
}

.empty-state-icon {
    font-size: 32px;
    margin-bottom: 4px;
}

.empty-state-title {
    font-size: 15px;
    font-weight: 500;
    color: var(--text-primary);
}

.empty-state-sub {
    font-size: 13px;
    color: var(--text-muted);
}

.recommendation-widget {
    background: var(--bg-elevated);
    border: 1px solid var(--accent-border);
    border-radius: var(--radius-lg);
    margin-bottom: 1.25rem;
    overflow: hidden;
    position: relative;
}

.recommendation-widget::before {
    content: '';
    position: absolute;
    inset: 0;
    background: radial-gradient(ellipse at 0% 0%, var(--accent-glow), transparent 60%);
    pointer-events: none;
}

.recommendation-widget-inner {
    padding: 18px 22px;
    display: flex;
    flex-direction: column;
    gap: 12px;
    position: relative;
}

.recommendation-widget-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--accent);
    text-transform: uppercase;
    letter-spacing: 0.08em;
}

.recommendation-widget-body {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

.recommendation-widget-title {
    font-size: 17px;
    font-weight: 600;
    color: var(--text-primary);
    letter-spacing: -0.01em;
}

.recommendation-widget-meta {
    font-size: 13px;
    color: var(--text-secondary);
    margin-top: 3px;
}

.modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
    padding-top: 4px;
}

/* Grade overview */
.grade-overview {
    margin-bottom: 2rem;
}

.grade-overview-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    margin-bottom: 12px;
}

.grade-overview-cards {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
}

.grade-overview-card {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius);
    padding: 14px 18px;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
    min-width: 100px;
    transition: border-color var(--transition);
}

.grade-overview-card:hover {
    border-color: var(--border-hover);
}

.grade-overview-subject {
    font-size: 12px;
    color: var(--text-muted);
    font-weight: 500;
}

.grade-overview-value {
    font-size: 22px;
    font-weight: 700;
    letter-spacing: -0.02em;
    font-variant-numeric: tabular-nums;
}

.grade-good { color: #4ade80; }
.grade-ok   { color: var(--accent); }
.grade-poor { color: #f87171; }

/* Study card bar */
.study-card-bar-bg {
    background: var(--bg-subtle);
    border-radius: 99px;
    height: 3px;
    width: 100%;
    overflow: hidden;
}

.study-card-bar {
    background: var(--accent);
    border-radius: 99px;
    height: 3px;
    transition: width 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 0 6px rgba(245, 158, 11, 0.4);
}

.content {
    padding-top: 0;
    max-width: 1100px;
    margin: 0 auto;
    width: 100%;
}
```

**Commit:**
```
design: redesign home page layout and components
```

---

## Commit 4 — Study page

### Replace contents of `Study.razor` with:

```razor
@page "/study"
@rendermode @(new InteractiveServerRenderMode(prerender: false))
<PageTitle>WSIST — Study</PageTitle>

<div class="page">
    <div class="top-row">
        <div class="title nav-brand">
            <img src="/logo.svg" width="26" height="26" alt="WSIST logo"/>
            <span>WSIST</span>
        </div>
        <a href="/" class="button-primary" data-enhance-nav="false">← Back</a>
        <a href="/logout" class="button-primary button-danger" data-enhance-nav="false">Logout</a>
    </div>

    <div class="content px-4">
        <div class="study-header">
            <h1>What should I study?</h1>
            <p>Tell us how much time you have — we'll figure out the rest.</p>
        </div>

        <div class="study-input-card">
            <label class="study-input-label">Hours available today</label>
            <div class="study-input-row">
                <input type="number" min="0.5" max="12" step="0.5" @bind="hoursAvailable"/>
                <button class="button-primary button-accent" @onclick="Calculate">Calculate</button>
            </div>
        </div>

        @if (calculated)
        {
            @if (recommendations.Count == 0)
            {
                <div class="empty-state" style="margin-top: 2rem">
                    <p class="empty-state-icon">🎉</p>
                    <p class="empty-state-title">You're all caught up</p>
                    <p class="empty-state-sub">No upcoming tests — enjoy your free time.</p>
                </div>
            }
            else
            {
                <div class="study-results">
                    <p class="study-results-label">Study these today</p>
                    @foreach (var test in recommendations)
                    {
                        var score = calculator.CalculateTotalScore(test, allTests);
                        var days = test.DueDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
                        <div class="study-card" title="@GetScoreBreakdown(test)">
                            <div class="study-card-header">
                                <div>
                                    <h3 class="study-card-title">@test.Title</h3>
                                    <p class="study-card-subject">
                                        @(subjects.FirstOrDefault(s => s.Id == test.Subject)?.Name ?? "Unknown") · in @days day@(days == 1 ? "" : "s")
                                    </p>
                                </div>
                                <span class="study-card-score">@score<span class="study-card-score-max">/40</span></span>
                            </div>
                            <p class="study-card-because">@GetBecauseText(test)</p>
                            <div class="study-card-bar-bg">
                                <div class="study-card-bar" style="width: @(score / 40.0 * 100)%"></div>
                            </div>
                        </div>
                    }
                </div>
            }
        }
    </div>
</div>
```

### Add to `app.css`:

```css
/* ── Study Page ── */

.study-header {
    padding: 1.5rem 0 1.25rem;
}

.study-header h1 {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.02em;
}

.study-header p {
    font-size: 13px;
    color: var(--text-muted);
    margin-top: 4px;
}

.study-input-card {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: 20px 22px;
    margin-bottom: 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.study-input-label {
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary);
    letter-spacing: 0.01em;
}

.study-input-row {
    display: flex;
    align-items: center;
    gap: 10px;
}

.study-input-row input {
    width: 100px;
    flex: none;
}

.study-results {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-bottom: 2rem;
}

.study-results-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    margin-bottom: 4px;
}

.study-card {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: 20px 22px;
    display: flex;
    flex-direction: column;
    gap: 12px;
    cursor: default;
    transition: border-color var(--transition);
}

.study-card:hover {
    border-color: var(--border-hover);
}

.study-card-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
}

.study-card-title {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    letter-spacing: -0.01em;
}

.study-card-subject {
    font-size: 13px;
    color: var(--text-secondary);
    margin-top: 3px;
}

.study-card-score {
    font-size: 22px;
    font-weight: 700;
    color: var(--accent);
    letter-spacing: -0.02em;
    font-variant-numeric: tabular-nums;
    flex-shrink: 0;
}

.study-card-score-max {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-muted);
}

.study-card-because {
    font-size: 13px;
    color: var(--text-secondary);
    line-height: 1.5;
}
```

**Commit:**
```
design: redesign study page
```

---

## Commit 5 — Settings page and landing page

### Replace contents of `Settings.razor` body (keep code-behind unchanged):

Replace the entire `Settings.razor` markup with:

```razor
@page "/settings"
@rendermode @(new InteractiveServerRenderMode(prerender: false))
<PageTitle>WSIST — Settings</PageTitle>

<div class="page">
    <div class="top-row">
        <div class="title nav-brand">
            <img src="/logo.svg" width="26" height="26" alt="WSIST logo"/>
            <span>WSIST</span>
        </div>
        <a href="/" class="button-primary" data-enhance-nav="false">← Back</a>
        <a href="/logout" class="button-primary button-danger" data-enhance-nav="false">Logout</a>
    </div>

    <div class="content px-4">
        <div class="settings-page-header">
            <h1>Settings</h1>
            <p>Manage your profile and subjects.</p>
        </div>

        @if (currentUser is not null)
        {
            <div class="settings-block">
                <p class="settings-block-label">Profile</p>
                <div class="settings-card">
                    <div class="settings-row">
                        <span class="settings-row-key">Display name</span>
                        <div class="settings-row-value">
                            <div class="settings-inline">
                                <input type="text" class="settings-input" @bind="editedDisplayName" @bind:event="oninput" maxlength="100"/>
                                <button class="button-primary button-accent" @onclick="SaveDisplayName">Save</button>
                            </div>
                            @if (saveMessage is not null)
                            {
                                <small class="settings-success">@saveMessage</small>
                            }
                        </div>
                    </div>
                    <div class="settings-row">
                        <span class="settings-row-key">Email</span>
                        <span class="settings-row-val">@currentUser.Email</span>
                    </div>
                    <div class="settings-row settings-row-last">
                        <span class="settings-row-key">Member since</span>
                        <span class="settings-row-val">@currentUser.CreatedAt.ToString("MMMM yyyy")</span>
                    </div>
                </div>
            </div>

            <div class="settings-block">
                <p class="settings-block-label">Subjects</p>
                <div class="settings-card">
                    @foreach (var subject in subjects)
                    {
                        <div class="subject-row @(subject == subjects.Last() && !subject.IsSystem ? "subject-row-last" : "")">
                            <span class="subject-name">@subject.Name</span>
                            @if (subject.IsSystem)
                            {
                                <span class="subject-badge">system</span>
                            }
                            else
                            {
                                <button class="button-primary button-danger" style="padding: 4px 10px; font-size: 12px;" @onclick="() => DeleteSubject(subject.Id)">Remove</button>
                            }
                        </div>
                    }
                    <div class="settings-add-subject">
                        <div class="settings-inline">
                            <input type="text" class="settings-input" placeholder="New subject name" @bind="newSubjectName" @bind:event="oninput" maxlength="100"/>
                            <button class="button-primary button-accent" @onclick="AddSubject">Add</button>
                        </div>
                        @if (subjectError is not null)
                        {
                            <small class="settings-error">@subjectError</small>
                        }
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

### Add to `app.css`:

```css
/* ── Settings Page ── */

.settings-page-header {
    padding: 1.5rem 0 1.25rem;
}

.settings-page-header h1 {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.02em;
}

.settings-page-header p {
    font-size: 13px;
    color: var(--text-muted);
    margin-top: 4px;
}

.settings-block {
    margin-bottom: 1.75rem;
}

.settings-block-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    margin-bottom: 10px;
}

.settings-card {
    background: var(--bg-elevated);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    overflow: hidden;
}

.settings-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 18px;
    border-bottom: 1px solid var(--border);
    gap: 16px;
    flex-wrap: wrap;
}

.settings-row-last {
    border-bottom: none;
}

.settings-row-key {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-secondary);
    flex-shrink: 0;
    min-width: 120px;
}

.settings-row-val {
    font-size: 14px;
    color: var(--text-primary);
}

.settings-row-value {
    display: flex;
    flex-direction: column;
    gap: 6px;
    flex: 1;
    min-width: 200px;
}

.settings-inline {
    display: flex;
    gap: 8px;
    align-items: center;
}

.settings-input {
    flex: 1;
    background: var(--bg);
    color: var(--text-primary);
    border: 1px solid var(--border);
    border-radius: var(--radius-sm);
    padding: 8px 12px;
    font-size: 14px;
    transition: border-color var(--transition), box-shadow var(--transition);
    outline: none;
}

.settings-input:focus {
    border-color: var(--accent);
    box-shadow: 0 0 0 3px var(--accent-dim);
}

.settings-success {
    color: #4ade80;
    font-size: 12px;
}

.settings-error {
    color: #f87171;
    font-size: 12px;
}

.subject-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 18px;
    border-bottom: 1px solid var(--border);
}

.subject-row-last {
    border-bottom: none;
}

.subject-name {
    font-size: 14px;
    color: var(--text-primary);
}

.subject-badge {
    font-size: 10px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--text-muted);
    background: var(--bg-subtle);
    border: 1px solid var(--border);
    border-radius: var(--radius-xs);
    padding: 2px 7px;
}

.settings-add-subject {
    padding: 14px 18px;
    display: flex;
    flex-direction: column;
    gap: 6px;
    border-top: 1px solid var(--border);
}
```

### Update the landing page hero in `Login.razor`

The landing page was rebuilt in Phase 3. Update only the hero section and overall aesthetic to match the new design system — same structure, but add the ambient glow and update the button styles to use the new accent color. Update the `.landing-hero-badge` color to `var(--accent)` and the `.landing-google-btn` to match `.button-primary.button-accent` styling. No structural changes needed — just verify the existing landing page CSS variables reference the same `:root` variables now updated in step 1.

**Commit:**
```
design: redesign settings page, align landing page to new design system
```

---

## Final checklist

- [ ] `dotnet build` passes with zero errors
- [ ] Font loads correctly (Plus Jakarta Sans visible in browser)
- [ ] Amber accent visible on buttons and score bars
- [ ] Nav has blur effect on scroll (sticky blur)
- [ ] Home table uses new dark row style with hover states
- [ ] Study cards show amber score bar with glow
- [ ] Modal has backdrop blur
- [ ] Settings rows are clean and consistent
- [ ] Landing page hero and CTA buttons use amber accent
- [ ] All pages use consistent spacing and typography

Push `feat/redesign` and open a PR.

---

## `/goal` for Fable 5

```
/goal Complete all five commits in @wsist-redesign-tasks.md in order. After each commit, run dotnet build and confirm zero errors before proceeding to the next. Do not deviate from the specified CSS values, color tokens, or component structure. The amber accent (#f59e0b), Plus Jakarta Sans font, and sticky blur nav are non-negotiable design decisions. After the final commit, run through the checklist at the bottom. Stop when all five commits are made and the checklist passes.
```
