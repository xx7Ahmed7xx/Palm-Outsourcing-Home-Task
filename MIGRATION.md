# OrderProcessor migration approach

## Context

OrderHub cannot go offline during the six-month move from .NET Framework 4.7 to .NET 8. Razor Pages and ASMX callers depend on `OrderProcessor` directly today, and August peak traffic makes a big-bang cutover risky.

## Approach: strangler fig behind a stable seam

1. **Extract a narrow contract first**  
   Introduce `IOrderProcessor` in the legacy solution with the same inputs/outputs as today (`schoolId`, lines, parent email → `OK` / `FAIL:*`). Keep WebForms and ASMX on that interface so callers do not change yet.

2. **Build the .NET 8 implementation in parallel**  
   Ship the new `OrderProcessor` (parameterised SQL, async I/O, injected dependencies) as a side-by-side library. Cover pricing, stock, and payment paths with unit tests before any production traffic hits it.

3. **Route traffic gradually with a feature flag**  
   In the legacy host, resolve `IOrderProcessor` from a factory:
   - default: existing Framework implementation
   - per school or percentage rollout: .NET 8 implementation via out-of-process HTTP/gRPC or in-proc if hosting allows

   Compare responses in shadow mode for a subset of schools before flipping the flag.

4. **Retire legacy code by vertical slice**  
   Once a school is on .NET 8, remove its branch from the monolith class. Repeat school-by-school through the back-to-school window rather than waiting for a single cutover weekend.

5. **Decommission ASMX last**  
   Replace direct class calls with the shared contract + API client, then delete the 1,800-line class when no caller references it.

## Why this fits

- **Zero downtime**: old and new processors run concurrently.
- **Rollback is instant**: flip the flag per school.
- **August risk is contained**: shadow traffic and phased rollout limit blast radius.
- **Q4-ready**: the extracted seams become natural service boundaries later.

## Risk to surface to leadership

**Dual-write / dual-read drift during shadow mode.** If legacy and .NET 8 processors read different stock or pricing data (cached tiers, stale replicas, or divergent SQL), we could approve orders in shadow that would fail in production—or vice versa. Agree upfront on a single source of truth per concern, freeze schema changes during rollout, and alert on shadow mismatches before enabling the flag for a school.
