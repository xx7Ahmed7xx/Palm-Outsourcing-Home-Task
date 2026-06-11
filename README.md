# OrderHub — Palm Outsourcing Trial Task (Job ID 681)

Ahmed's submission for the Sr. Software Engineer trial task (Brindleford & Co / OrderHub scenario).

## Structure

| Path | Question |
|------|----------|
| `src/OrderHub.Application/` | Q1 — .NET 8 `OrderProcessor` |
| `src/OrderHub.Infrastructure/` | Q1 — SQLite repositories (parameterised SQL) |
| `MIGRATION.md` | Q1 — migration plan |
| `tests/OrderHub.Tests/` | Q1 — pricing business-rule tests |
| `src/OrderHub.Web/Pages/ConfirmOrder.*` | Q3 — Razor Page rewrite |
| `src/OrderHub.Web/wwwroot/js/confirm-order.js` | Q3 — vanilla JS live subtotal |
| `docs/Q3-ISSUES.md` | Q3 — three issues write-up |
| `docs/Q2-sql-analytics.md` | Q2 — source for GitHub Gist |
| `docs/Q4-microservices.md` | Q4 — source for GitHub Gist |

## Run locally

```bash
dotnet test
dotnet run --project src/OrderHub.Web
```

Open `/DemoOrder` to run the processor against SQLite, or `/ConfirmOrder` for Q3, check `docs/Q3-ISSUES.md` for more.

## Submission checklist

- [ ] Push to a **public** GitHub repo (Q1 + Q3)
- [ ] Publish `docs/Q2-sql-analytics.md` as a **public Gist** (Q2)
- [ ] Publish `docs/Q4-microservices.md` as a **public Gist** (Q4)
- [ ] Record Loom (camera on, ≤3 min)
- [ ] Submit form with Job ID **681**
