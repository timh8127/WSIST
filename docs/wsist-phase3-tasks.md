# WSIST — Phase 3 Task File (Claude Fable 5)

## Instructions for the model

Work through every task in the order listed. Do not stop to ask for clarification — every decision is specified below. Commit after each task using the exact message provided. If a file already exists, edit it in place rather than replacing it. Run `dotnet build` before each commit to confirm no compilation errors.

Branch to create before starting: `feat/phase3-polish`

---

## Task 1 — SVG Logo

**What:** Create a minimal SVG logo for WSIST that works as both a favicon and an inline nav element. Use the existing accent color (`#5b8cff`).

**Design spec:** The letter "W" rendered in a clean geometric style, with a small upward-pointing chevron above it to suggest growth/progress. Dark background circle behind it for use as a favicon.

### Step 1 — Create `WSIST/WSIST.Web/wwwroot/logo.svg`

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none">
  <!-- Background circle -->
  <circle cx="32" cy="32" r="32" fill="#0f1115"/>
  <!-- Chevron accent -->
  <polyline points="22,20 32,12 42,20" stroke="#5b8cff" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
  <!-- W letterform -->
  <polyline points="16,28 22,48 32,36 42,48 48,28" stroke="#e6e8eb" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
</svg>
```

### Step 2 — Replace the favicon reference in `App.razor`

Replace:
```razor
<link rel="icon" type="image/png" href="favicon.png"/>
```
With:
```razor
<link rel="icon" type="image/svg+xml" href="logo.svg"/>
```

### Step 3 — Update the nav title in `Home.razor`, `Study.razor`, and `Settings.razor`

In every `top-row` div, replace the plain text `<h3 class="title">WSIST</h3>` with:

```razor
<div class="title nav-brand">
    <img src="/logo.svg" width="28" height="28" alt="WSIST logo" />
    <span>WSIST</span>
</div>
```

Add to `app.css`:

```css
.nav-brand {
    display: flex;
    align-items: center;
    gap: 10px;
}

.nav-brand span {
    font-size: 18px;
    font-weight: 700;
    color: var(--text-primary);
    letter-spacing: -0.02em;
}
```

**Commit:**
```
feat: add SVG logo, update favicon and nav branding
```

---

## Task 2 — Landing Page Revamp

**What:** Replace the minimal login card at `/login-page` with a proper full-page marketing landing page. Keep the `@layout EmptyLayout` directive. Do not remove or alter the Google login endpoint — only the visual presentation changes.

The page must have five sections: Nav, Hero, Features, How It Works, Footer.

### Replace the full contents of `WSIST/WSIST.Web/Components/Pages/Login.razor` with:

```razor
@page "/login-page"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>WSIST – What Should I Study Today?</PageTitle>

<div class="landing">

    <!-- Nav -->
    <nav class="landing-nav">
        <div class="landing-nav-brand">
            <img src="/logo.svg" width="32" height="32" alt="WSIST" />
            <span>WSIST</span>
        </div>
        <a href="/login" class="button-primary landing-cta-sm" data-enhance-nav="false">
            Get started
        </a>
    </nav>

    <!-- Hero -->
    <section class="landing-hero">
        <div class="landing-hero-inner">
            <div class="landing-hero-badge">Study smarter, not harder</div>
            <h1 class="landing-hero-title">
                Know exactly what<br/>to study today.
            </h1>
            <p class="landing-hero-sub">
                WSIST tracks your upcoming tests, measures how prepared you are,
                and tells you where to focus — so you never waste a study session again.
            </p>
            <a href="/login" class="button-primary landing-google-btn" data-enhance-nav="false">
                <img src="https://www.google.com/favicon.ico" width="18" height="18" alt="Google" />
                Continue with Google — it's free
            </a>
            <p class="landing-hero-note">No credit card. No setup. Just log in.</p>
        </div>
    </section>

    <!-- Features -->
    <section class="landing-features">
        <div class="landing-section-inner">
            <h2 class="landing-section-title">Everything you need to prepare</h2>
            <div class="landing-feature-grid">
                <div class="landing-feature-card">
                    <span class="landing-feature-icon">📋</span>
                    <h3>Track every test</h3>
                    <p>Add tests with their due date, topic volume, and your current understanding level. WSIST keeps the full picture in one place.</p>
                </div>
                <div class="landing-feature-card">
                    <span class="landing-feature-icon">🎯</span>
                    <h3>Smart priority scoring</h3>
                    <p>A priority engine weighs urgency, workload, understanding, and past grades to rank what matters most — automatically.</p>
                </div>
                <div class="landing-feature-card">
                    <span class="landing-feature-icon">📚</span>
                    <h3>Daily study plan</h3>
                    <p>Tell WSIST how many hours you have. It returns the tests you should study today, ranked and explained.</p>
                </div>
                <div class="landing-feature-card">
                    <span class="landing-feature-icon">📊</span>
                    <h3>Grade overview</h3>
                    <p>See your average grade per subject at a glance. Spot weak areas before they become a problem.</p>
                </div>
            </div>
        </div>
    </section>

    <!-- How It Works -->
    <section class="landing-how">
        <div class="landing-section-inner">
            <h2 class="landing-section-title">Three steps to a better study session</h2>
            <div class="landing-steps">
                <div class="landing-step">
                    <div class="landing-step-number">1</div>
                    <div class="landing-step-content">
                        <h3>Add your tests</h3>
                        <p>Enter each upcoming test with its subject, date, how much material there is, and how well you understand it.</p>
                    </div>
                </div>
                <div class="landing-step-connector"></div>
                <div class="landing-step">
                    <div class="landing-step-number">2</div>
                    <div class="landing-step-content">
                        <h3>Set your available time</h3>
                        <p>Each day, tell WSIST how many hours you can study. Even 30 minutes counts.</p>
                    </div>
                </div>
                <div class="landing-step-connector"></div>
                <div class="landing-step">
                    <div class="landing-step-number">3</div>
                    <div class="landing-step-content">
                        <h3>Study what matters</h3>
                        <p>WSIST shows you exactly which test to focus on and why — no more guessing, no more guilt.</p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Final CTA -->
    <section class="landing-final-cta">
        <div class="landing-section-inner landing-final-cta-inner">
            <h2>Ready to stop guessing?</h2>
            <p>Join students who always know what to study next.</p>
            <a href="/login" class="button-primary landing-google-btn" data-enhance-nav="false">
                <img src="https://www.google.com/favicon.ico" width="18" height="18" alt="Google" />
                Continue with Google
            </a>
        </div>
    </section>

    <!-- Footer -->
    <footer class="landing-footer">
        <div class="landing-footer-inner">
            <span class="landing-footer-brand">WSIST</span>
            <div class="landing-footer-links">
                <a href="/privacy">Privacy</a>
                <a href="/terms">Terms</a>
            </div>
            <span class="landing-footer-copy">© @DateTime.Now.Year Tim Hug</span>
        </div>
    </footer>
</div>
```

### Add to `app.css`:

```css
/* ── Landing Page ── */

.landing {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background: var(--bg);
}

.landing-nav {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1.25rem 2rem;
    border-bottom: 1px solid var(--border-subtle);
    position: sticky;
    top: 0;
    background: var(--bg);
    z-index: 100;
}

.landing-nav-brand {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 20px;
    font-weight: 700;
    color: var(--text-primary);
    letter-spacing: -0.02em;
}

.landing-cta-sm {
    text-decoration: none;
    font-size: 14px;
    padding: 8px 16px;
}

.landing-hero {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 7rem 2rem 6rem;
    text-align: center;
    flex: 1;
}

.landing-hero-inner {
    max-width: 640px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 20px;
}

.landing-hero-badge {
    background: rgba(91, 140, 255, 0.12);
    color: var(--accent);
    border: 1px solid rgba(91, 140, 255, 0.25);
    border-radius: 99px;
    padding: 5px 14px;
    font-size: 13px;
    font-weight: 500;
    letter-spacing: 0.02em;
}

.landing-hero-title {
    font-size: clamp(2.2rem, 5vw, 3.5rem);
    font-weight: 800;
    color: var(--text-primary);
    line-height: 1.1;
    letter-spacing: -0.03em;
    margin: 0;
}

.landing-hero-sub {
    font-size: 1.1rem;
    color: var(--text-secondary);
    line-height: 1.6;
    max-width: 500px;
    margin: 0;
}

.landing-google-btn {
    display: inline-flex;
    align-items: center;
    gap: 10px;
    text-decoration: none;
    font-size: 16px;
    padding: 14px 28px;
    border-radius: 10px;
    font-weight: 600;
}

.landing-hero-note {
    font-size: 13px;
    color: var(--text-secondary);
    margin: 0;
}

.landing-section-inner {
    max-width: 900px;
    margin: 0 auto;
    padding: 5rem 2rem;
    width: 100%;
}

.landing-section-title {
    font-size: 1.8rem;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 2.5rem;
    letter-spacing: -0.02em;
}

.landing-features {
    background: var(--bg-elevated);
    border-top: 1px solid var(--border-subtle);
    border-bottom: 1px solid var(--border-subtle);
}

.landing-feature-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 24px;
}

.landing-feature-card {
    background: var(--bg);
    border: 1px solid var(--border-subtle);
    border-radius: 12px;
    padding: 24px;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.landing-feature-icon {
    font-size: 28px;
}

.landing-feature-card h3 {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 0;
}

.landing-feature-card p {
    font-size: 14px;
    color: var(--text-secondary);
    line-height: 1.5;
    margin: 0;
}

.landing-how {
    background: var(--bg);
}

.landing-steps {
    display: flex;
    flex-direction: column;
    gap: 0;
}

.landing-step {
    display: flex;
    align-items: flex-start;
    gap: 20px;
}

.landing-step-connector {
    width: 2px;
    height: 32px;
    background: var(--border-subtle);
    margin-left: 19px;
}

.landing-step-number {
    width: 40px;
    height: 40px;
    min-width: 40px;
    border-radius: 50%;
    background: rgba(91, 140, 255, 0.12);
    border: 1px solid rgba(91, 140, 255, 0.3);
    color: var(--accent);
    font-weight: 700;
    font-size: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.landing-step-content {
    padding-top: 6px;
    padding-bottom: 8px;
}

.landing-step-content h3 {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 0 0 6px;
}

.landing-step-content p {
    font-size: 14px;
    color: var(--text-secondary);
    line-height: 1.5;
    margin: 0;
}

.landing-final-cta {
    background: var(--bg-elevated);
    border-top: 1px solid var(--border-subtle);
    text-align: center;
}

.landing-final-cta-inner {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
}

.landing-final-cta h2 {
    font-size: 2rem;
    font-weight: 700;
    color: var(--text-primary);
    letter-spacing: -0.02em;
    margin: 0;
}

.landing-final-cta p {
    color: var(--text-secondary);
    margin: 0;
}

.landing-footer {
    border-top: 1px solid var(--border-subtle);
    padding: 1.5rem 2rem;
}

.landing-footer-inner {
    max-width: 900px;
    margin: 0 auto;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 12px;
}

.landing-footer-brand {
    font-weight: 700;
    color: var(--text-primary);
    font-size: 15px;
}

.landing-footer-links {
    display: flex;
    gap: 20px;
}

.landing-footer-links a {
    font-size: 13px;
    color: var(--text-secondary);
    text-decoration: none;
}

.landing-footer-links a:hover {
    color: var(--text-primary);
}

.landing-footer-copy {
    font-size: 13px;
    color: var(--text-secondary);
}
```

**Commit:**
```
feat: revamp landing page with hero, features, and how-it-works sections
```

---

## Task 3 — Privacy Policy Page

**What:** Create a GDPR and Swiss nDSG compliant privacy policy page at `/privacy`. Uses `EmptyLayout`. No authentication required.

**Note:** Replace `[YOUR_EMAIL]` in the content below with the actual contact email — if it is not known, use `privacy@wsist.forch.me` as a placeholder.

### Create `WSIST/WSIST.Web/Components/Pages/Privacy.razor`:

```razor
@page "/privacy"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>Privacy Policy – WSIST</PageTitle>

<div class="legal-page">
    <nav class="landing-nav">
        <div class="landing-nav-brand">
            <img src="/logo.svg" width="32" height="32" alt="WSIST" />
            <span>WSIST</span>
        </div>
        <a href="/login-page" class="landing-cta-sm" style="color: var(--text-secondary); text-decoration: none;">← Back</a>
    </nav>

    <div class="legal-content">
        <h1>Privacy Policy</h1>
        <p class="legal-meta">Last updated: @(new DateTime(2026, 6, 1).ToString("MMMM d, yyyy"))</p>

        <h2>1. Who we are</h2>
        <p>WSIST ("What Should I Study Today") is a personal study planning application developed and operated by Tim Hug, based in Switzerland. This privacy policy explains what personal data we collect, how we use it, and what rights you have.</p>
        <p>Contact: <a href="mailto:privacy@wsist.forch.me">privacy@wsist.forch.me</a></p>

        <h2>2. Data we collect</h2>
        <p>When you sign in with Google, we receive and store the following data from Google's OAuth service:</p>
        <ul>
            <li><strong>Email address</strong> — used to identify your account</li>
            <li><strong>Display name</strong> — shown in the application</li>
            <li><strong>Google user ID</strong> — used to link your Google account to your WSIST account</li>
        </ul>
        <p>We also store data you create while using the application:</p>
        <ul>
            <li>Test entries (title, subject, due date, volume, understanding level, grade)</li>
            <li>Custom subjects you create</li>
            <li>Your display name if you update it in settings</li>
            <li>Your account creation timestamp</li>
        </ul>
        <p>We do not collect location data, device identifiers, or any data beyond what is listed above.</p>

        <h2>3. How we use your data</h2>
        <p>Your data is used solely to provide the WSIST service:</p>
        <ul>
            <li>Authenticating you when you log in</li>
            <li>Storing and displaying your test entries</li>
            <li>Calculating study recommendations and priority scores</li>
            <li>Showing your grade overview</li>
        </ul>
        <p>We do not sell your data, share it with advertisers, or use it for any purpose other than providing the service.</p>

        <h2>4. Legal basis for processing (GDPR / Swiss nDSG)</h2>
        <p>We process your personal data on the basis of <strong>contract performance</strong> (Art. 6(1)(b) GDPR) — processing is necessary to provide the service you have requested. By creating an account and using WSIST, you enter into a contract with us for the provision of the application.</p>

        <h2>5. Third-party services</h2>
        <p><strong>Google OAuth:</strong> Login is handled via Google's OAuth 2.0 service. When you authenticate, Google shares your profile data with us as described in section 2. Google's own privacy policy applies to the authentication process: <a href="https://policies.google.com/privacy" target="_blank" rel="noopener">policies.google.com/privacy</a>.</p>
        <p><strong>Railway:</strong> The application is hosted on Railway (railway.app). Your data is stored on a MySQL database on their infrastructure. Railway's privacy policy applies to hosting: <a href="https://railway.app/legal/privacy" target="_blank" rel="noopener">railway.app/legal/privacy</a>.</p>
        <p><strong>Cloudflare:</strong> The application is served behind Cloudflare's CDN. Cloudflare may process connection metadata (IP address, request headers) as part of its service. Cloudflare's privacy policy: <a href="https://www.cloudflare.com/privacypolicy/" target="_blank" rel="noopener">cloudflare.com/privacypolicy</a>.</p>

        <h2>6. Data retention</h2>
        <p>Your data is retained for as long as you have an active account. If you wish to delete your account and all associated data, contact us at <a href="mailto:privacy@wsist.forch.me">privacy@wsist.forch.me</a> and we will delete it within 30 days.</p>

        <h2>7. Your rights</h2>
        <p>Under the GDPR and Swiss nDSG, you have the following rights:</p>
        <ul>
            <li><strong>Access:</strong> You can request a copy of all personal data we hold about you.</li>
            <li><strong>Rectification:</strong> You can correct inaccurate data via the Settings page or by contacting us.</li>
            <li><strong>Erasure:</strong> You can request deletion of your account and all associated data.</li>
            <li><strong>Portability:</strong> You can request your data in a machine-readable format.</li>
            <li><strong>Objection:</strong> You can object to processing in certain circumstances.</li>
        </ul>
        <p>To exercise any of these rights, contact us at <a href="mailto:privacy@wsist.forch.me">privacy@wsist.forch.me</a>. We will respond within 30 days.</p>

        <h2>8. Cookies and sessions</h2>
        <p>WSIST uses a single authentication cookie (`.AspNetCore.Cookies`) to maintain your login session. This cookie is strictly necessary for the application to function and does not track you across other websites. No analytics, advertising, or third-party tracking cookies are used.</p>

        <h2>9. Security</h2>
        <p>Your data is transmitted over HTTPS. Passwords are never stored — authentication is handled entirely by Google. Database access is restricted to the application only.</p>

        <h2>10. Changes to this policy</h2>
        <p>If we make material changes to this policy, we will update the "last updated" date at the top of this page. Continued use of WSIST after changes constitutes acceptance of the updated policy.</p>

        <h2>11. Contact</h2>
        <p>Questions about this privacy policy? Contact us at <a href="mailto:privacy@wsist.forch.me">privacy@wsist.forch.me</a>.</p>
    </div>
</div>
```

### Add to `app.css`:

```css
/* ── Legal Pages ── */

.legal-page {
    min-height: 100vh;
    background: var(--bg);
    display: flex;
    flex-direction: column;
}

.legal-content {
    max-width: 720px;
    margin: 0 auto;
    padding: 4rem 2rem;
    width: 100%;
}

.legal-content h1 {
    font-size: 2rem;
    font-weight: 800;
    color: var(--text-primary);
    letter-spacing: -0.02em;
    margin-bottom: 0.5rem;
}

.legal-meta {
    color: var(--text-secondary);
    font-size: 14px;
    margin-bottom: 2.5rem;
}

.legal-content h2 {
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--text-primary);
    margin-top: 2rem;
    margin-bottom: 0.75rem;
}

.legal-content p,
.legal-content li {
    font-size: 15px;
    color: var(--text-secondary);
    line-height: 1.7;
    margin-bottom: 0.75rem;
}

.legal-content ul {
    padding-left: 1.5rem;
}

.legal-content a {
    color: var(--accent);
    text-decoration: none;
}

.legal-content a:hover {
    text-decoration: underline;
}

.legal-content strong {
    color: var(--text-primary);
    font-weight: 600;
}
```

**Commit:**
```
feat: add GDPR and nDSG compliant privacy policy page
```

---

## Task 4 — Terms of Use Page

**What:** Create a Terms of Use page at `/terms`. Uses `EmptyLayout`. No authentication required.

### Create `WSIST/WSIST.Web/Components/Pages/Terms.razor`:

```razor
@page "/terms"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>Terms of Use – WSIST</PageTitle>

<div class="legal-page">
    <nav class="landing-nav">
        <div class="landing-nav-brand">
            <img src="/logo.svg" width="32" height="32" alt="WSIST" />
            <span>WSIST</span>
        </div>
        <a href="/login-page" class="landing-cta-sm" style="color: var(--text-secondary); text-decoration: none;">← Back</a>
    </nav>

    <div class="legal-content">
        <h1>Terms of Use</h1>
        <p class="legal-meta">Last updated: @(new DateTime(2026, 6, 1).ToString("MMMM d, yyyy"))</p>

        <h2>1. Acceptance of terms</h2>
        <p>By accessing or using WSIST ("What Should I Study Today") at wsist.forch.me, you agree to be bound by these Terms of Use. If you do not agree, do not use the service.</p>

        <h2>2. Description of service</h2>
        <p>WSIST is a personal study planning tool that helps students track upcoming tests, estimate their preparedness, and determine what to study on a given day. The service is provided free of charge for personal, non-commercial use.</p>

        <h2>3. Account and access</h2>
        <p>Access to WSIST requires a Google account. By signing in, you authorise WSIST to receive your name and email address from Google as described in our <a href="/privacy">Privacy Policy</a>. You are responsible for keeping your Google account secure.</p>
        <p>One account per person. You may not share your account with others or create accounts on behalf of third parties.</p>

        <h2>4. Acceptable use</h2>
        <p>You agree not to:</p>
        <ul>
            <li>Use WSIST for any unlawful purpose</li>
            <li>Attempt to access other users' data</li>
            <li>Attempt to reverse-engineer, disrupt, or overload the service</li>
            <li>Use automated tools to scrape or extract data from the service</li>
        </ul>

        <h2>5. Your content</h2>
        <p>You own the data you enter into WSIST (test entries, subjects, grades). We do not claim any ownership over it. You grant us a limited licence to store and process it solely for the purpose of providing the service to you.</p>

        <h2>6. Service availability</h2>
        <p>WSIST is a personal project and is provided on an "as is" and "as available" basis. We make no guarantees about uptime, data preservation, or continued availability. We may modify or discontinue the service at any time without notice.</p>

        <h2>7. Disclaimer of warranties</h2>
        <p>To the fullest extent permitted by law, WSIST is provided without warranties of any kind, express or implied. We do not warrant that the service will be error-free, uninterrupted, or that the study recommendations it produces will lead to any particular academic outcome.</p>

        <h2>8. Limitation of liability</h2>
        <p>To the fullest extent permitted by applicable law, Tim Hug and the WSIST project shall not be liable for any indirect, incidental, or consequential damages arising from your use of or inability to use the service.</p>

        <h2>9. Changes to terms</h2>
        <p>We may update these terms at any time. The "last updated" date at the top of this page will reflect any changes. Continued use of WSIST after changes are posted constitutes acceptance of the updated terms.</p>

        <h2>10. Governing law</h2>
        <p>These terms are governed by the laws of Switzerland, without regard to conflict of law principles. Any disputes arising from these terms shall be subject to the exclusive jurisdiction of the courts of the Canton of Zurich, Switzerland.</p>

        <h2>11. Contact</h2>
        <p>Questions about these terms? Contact us at <a href="mailto:privacy@wsist.forch.me">privacy@wsist.forch.me</a>.</p>
    </div>
</div>
```

**Commit:**
```
feat: add terms of use page
```

---

## Task 5 — EduSync Grade Average API Endpoint

**What:** Add a read-only API endpoint that exposes a user's average grade per subject. This is the data contract for potential EduSync integration. The endpoint requires a valid authenticated session cookie — no new auth mechanism needed.

The endpoint returns JSON in the format:
```json
{
  "userId": 3,
  "subjects": [
    { "subjectId": 0, "subjectName": "Math", "averageGrade": 4.8, "gradedTestCount": 3 },
    { "subjectId": 1, "subjectName": "English", "averageGrade": 5.1, "gradedTestCount": 2 }
  ]
}
```

Only subjects with at least one graded test are included.

### Step 1 — Add `GetGradeAverages` to `TestManagement.cs`

```csharp
public record SubjectGradeAverage(int SubjectId, string SubjectName, double AverageGrade, int GradedTestCount);

public List<SubjectGradeAverage> GetGradeAverages(int userId)
{
    var subjects = context.Subjects
        .Where(s => s.IsSystem || s.UserId == userId)
        .ToList();

    return context.Tests
        .Where(t => t.UserId == userId && t.Grade != null)
        .AsEnumerable()
        .GroupBy(t => t.Subject)
        .Select(g =>
        {
            var subject = subjects.FirstOrDefault(s => s.Id == g.Key);
            return new SubjectGradeAverage(
                g.Key,
                subject?.Name ?? g.Key.ToString(),
                Math.Round(g.Average(t => t.Grade!.Value), 2),
                g.Count()
            );
        })
        .OrderBy(x => x.SubjectName)
        .ToList();
}
```

### Step 2 — Register the endpoint in `Program.cs`

Add after the existing `app.MapGet("/logout", ...)` block:

```csharp
app.MapGet("/api/grades", async (HttpContext ctx, TestManagement management) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
    if (email is null) return Results.Unauthorized();

    var user = management.GetOrCreateUser(
        email,
        ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
        ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""
    );

    var averages = management.GetGradeAverages(user.Id);

    return Results.Ok(new
    {
        userId = user.Id,
        subjects = averages
    });
}).RequireAuthorization();
```

Add the missing `using System.Security.Claims;` at the top of `Program.cs` if it is not already present.

### Verify

Run the app, log in, then navigate to `/api/grades`. If you have graded tests, the JSON response should list average grades per subject. If no graded tests exist, the `subjects` array will be empty — that is correct.

**Commit:**
```
feat: add /api/grades endpoint for EduSync integration
```

---

## Final checklist before pushing

- [ ] `dotnet build` passes with no errors
- [ ] Landing page renders at `/login-page` when not logged in
- [ ] Privacy page renders at `/privacy` without authentication
- [ ] Terms page renders at `/terms` without authentication
- [ ] `/api/grades` returns JSON when authenticated
- [ ] Logo appears in the nav bar on all three app pages
- [ ] Footer links on landing page route to `/privacy` and `/terms`

Push `feat/phase3-polish` and open a PR.

---

## Suggested `/goal` invocation

```
/goal Complete all five tasks in @wsist-phase3-tasks.md in order. Run dotnet build before each commit and confirm it passes. Do not ask for clarification — all design and content decisions are fully specified in the file. After the final commit, run through the checklist at the bottom of the file and confirm each item. Stop when all five commits are made and the checklist is clear.
```

Model string for Claude Code: `claude-fable-5`
