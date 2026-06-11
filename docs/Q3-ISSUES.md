# Q3 — Three issues that matter most for OrderHub

## 1. `@Html.Raw(Model.SchoolName)` — cross-site scripting (XSS)

School names are admin-entered data. Rendering them with `Html.Raw` turns a malicious or compromised school name into executable script in the admin browser. During back-to-school, dozens of school admins use this page daily; one stored XSS payload could capture session cookies or trigger actions on their behalf.

**Fix:** encode all user-supplied text (`@Model.SchoolName`) and reserve raw HTML only for trusted, sanitised fragments.

## 2. Full form POST on every quantity change

`updateQty` submits the entire form whenever a quantity changes. That forces a full round-trip, re-validates nothing client-side, and feels broken during peak season when admins are adjusting multiple lines. Worse, a slow August response looks like a hang—exactly the duplicate-submit scenario flagged in Q4.

**Fix:** update the subtotal in the browser; persist quantity changes on explicit Confirm or via a targeted API call.

## 3. Fragile inline JavaScript (`document.forms[0]`)

The handler assumes the confirm form is always `forms[0]`. Adding a search box, filter form, or layout partial breaks quantity updates silently. Inline script also bypasses CSP and makes the page harder to test.

**Fix:** move interaction to a dedicated `.js` file, bind by element id/data-attribute, and keep the Razor page for structure only.

## Follow-up fixes applied

- **Server-side binding on Confirm** — quantities post back via `asp-for`, persist to SQLite, and subtotal is recalculated on the server.
- **Anti-forgery** — `[ValidateAntiForgeryToken]` on the POST handler (token emitted by the form tag helper).
- **Double-submit guard** — Confirm button disables and shows "Confirming…" on submit.
