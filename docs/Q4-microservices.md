# Q4 — Decomposition and confirmation workflow

## Service boundaries

| Service | Owns | Contract (sync) | Transaction boundary |
|---------|------|-------------------|----------------------|
| **School Service** | Schools, tier codes | `GET /schools/{id}/tier` | Read-only |
| **Catalog Service** | Products, base prices, categories | `GET /products/{sku}` | Read-only |
| **Inventory Service** | Stock levels, reservations | `POST /stock/reserve` (idempotent) | **Writes stock** — reservation is its own transaction |
| **Pricing Service** | Tier rules, embroidery fees | `POST /price/quote` (stateless) | None — pure calculation |
| **Payment Service** | Payment intents, webhooks | `POST /payments/intents` | **Owns payment state** |
| **Notification Service** | Confirmation email/SMS | `POST /notifications/order-confirmed` | **At-least-once send** — separate from payment |
| **Order Orchestrator** | Order aggregate, saga state | `POST /orders` (workflow entry) | **Owns order lifecycle** — coordinates, does not hold inventory rows |

**Flow:** Orchestrator calls School → Catalog → Pricing → Inventory.reserve → Payment.create. Each downstream call carries an `Idempotency-Key` derived from `orderId` + step name. Inventory reserve and payment are **not** in one DB transaction; compensating actions release stock if payment fails.

```
Admin UI → Order Orchestrator → [School, Catalog, Pricing] (read)
                              → Inventory (reserve)
                              → Payment (intent)
                              → Notification (async, after payment confirmed)
```

## Confirmation flow (~150 words)

On **Submit**, the admin UI POSTs to **Order Orchestrator** with an `Idempotency-Key` header (client-generated UUID). Orchestrator creates order `PENDING`, publishes `OrderSubmitted` to **Azure Service Bus**, returns `202 Accepted` + `orderId`.

Workers consume events: validate → reserve stock → create payment intent → on `PaymentSucceeded` webhook, mark order `CONFIRMED` and publish `OrderConfirmed`.

**Retries:** exponential backoff, max 5 attempts, dead-letter after. All handlers idempotent via `orderId` + event type.

**Payment succeeds, email fails:** order stays `CONFIRMED`; notification retries independently. Admin sees confirmation in UI; email lands async. Alert if notification DLQ exceeds threshold—ops resend manually; never roll back payment.

## On-call runbook — scenario (a): duplicate submit after hang

**Alert:** two `POST /orders` with same payload within 30s, or support ticket "double charge fear".

**Triage (5 min):** Look up both requests by `Idempotency-Key` or hash(parentEmail + schoolId + line fingerprint + 5-min window). If keys match, second request should return original `orderId` with `200`—no duplicate charge.

**If keys differ (admin clicked twice):** compare `orderId` states. If first is `PENDING` >60s, check payment + inventory logs—not admin error.

**Mitigate:** confirm single payment intent; if duplicate intents exist, void the orphan via Payment Service. Release duplicate stock reservation.

**Comms (Slack template):** "Checked order {id}—one charge, one reservation. Refresh confirmed page; ignore second spinner."

**Follow-up:** verify UI disables Submit after click; extend idempotency window messaging in Q3.
