# Q2 — SQL Server schema, indexes, and analytics query

## Schema (minimal columns for YoY revenue by school, season, category)

```sql
CREATE TABLE Schools (
    Id           INT          NOT NULL PRIMARY KEY,
    Name         NVARCHAR(200) NOT NULL
);

CREATE TABLE Products (
    Sku          NVARCHAR(50)  NOT NULL PRIMARY KEY,
    Category     NVARCHAR(100) NOT NULL
);

CREATE TABLE Orders (
    Id           BIGINT       NOT NULL PRIMARY KEY,
    SchoolId     INT          NOT NULL,
    SeasonYear   SMALLINT     NOT NULL,  -- e.g. 2025 for "Back to School 2025"
    OrderedAt    DATETIME2(0) NOT NULL,
    CONSTRAINT FK_Orders_Schools FOREIGN KEY (SchoolId) REFERENCES Schools (Id)
);

CREATE TABLE OrderLines (
    Id           BIGINT        NOT NULL PRIMARY KEY,
    OrderId      BIGINT        NOT NULL,
    ProductSku   NVARCHAR(50)  NOT NULL,
    Quantity     INT           NOT NULL,
    UnitPrice    DECIMAL(10,2) NOT NULL,  -- price at time of order
    CONSTRAINT FK_OrderLines_Orders   FOREIGN KEY (OrderId)    REFERENCES Orders (Id),
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductSku) REFERENCES Products (Sku),
    CONSTRAINT CK_OrderLines_Qty      CHECK (Quantity > 0),
    CONSTRAINT CK_OrderLines_Price      CHECK (UnitPrice >= 0)
);
```

## Indexes

```sql
-- Drive the join path Orders → OrderLines → Products and filter/group by school + season
CREATE INDEX IX_Orders_School_Season
    ON Orders (SchoolId, SeasonYear)
    INCLUDE (OrderedAt);

CREATE INDEX IX_OrderLines_OrderId
    ON OrderLines (OrderId)
    INCLUDE (ProductSku, Quantity, UnitPrice);

CREATE INDEX IX_Products_Category
    ON Products (Category)
    INCLUDE (Sku);
```

**Trade-off:** I index `Orders` on `(SchoolId, SeasonYear)` rather than `(SeasonYear, SchoolId)` because the analytics UI is typically filtered by school first during admin review. At 8M lines, leading with the higher-cardinality dimension that matches the UI filter reduces key lookups.

## Analytics query — year-on-year revenue by school, season, and category

```sql
DECLARE @CurrentSeason SMALLINT = 2025;
DECLARE @PriorSeason   SMALLINT = 2024;

WITH RevenueBySlice AS (
    SELECT
        s.Id            AS SchoolId,
        s.Name          AS SchoolName,
        o.SeasonYear,
        p.Category,
        SUM(ol.Quantity * ol.UnitPrice) AS Revenue
    FROM Orders o
    INNER JOIN Schools s   ON s.Id = o.SchoolId
    INNER JOIN OrderLines ol ON ol.OrderId = o.Id
    INNER JOIN Products p  ON p.Sku = ol.ProductSku
    WHERE o.SeasonYear IN (@CurrentSeason, @PriorSeason)
    GROUP BY
        s.Id, s.Name, o.SeasonYear, p.Category
)
SELECT
    curr.SchoolId,
    curr.SchoolName,
    curr.Category,
    curr.Revenue                              AS CurrentSeasonRevenue,
    ISNULL(prior.Revenue, 0)                  AS PriorSeasonRevenue,
    curr.Revenue - ISNULL(prior.Revenue, 0)   AS RevenueDelta,
    CASE
        WHEN ISNULL(prior.Revenue, 0) = 0 THEN NULL
        ELSE ROUND((curr.Revenue - prior.Revenue) * 100.0 / prior.Revenue, 1)
    END AS YoYPercentChange
FROM RevenueBySlice curr
LEFT JOIN RevenueBySlice prior
    ON  prior.SchoolId   = curr.SchoolId
    AND prior.Category   = curr.Category
    AND prior.SeasonYear = @PriorSeason
WHERE curr.SeasonYear = @CurrentSeason
ORDER BY curr.SchoolName, curr.Category;
```

**Trade-offs noted in-query:**
- Uses stored `UnitPrice` on `OrderLines` (order-time truth) instead of joining current `Products` price — correct for historical revenue, slightly more storage.
- Self-join on the CTE keeps the query readable; at 5× volume I would materialise a nightly `SalesBySchoolSeasonCategory` aggregate table and query that instead.

## Anti-pattern flag (Slack message)

> **Heads-up — correlated subquery per row on `OrderLines`**
>
> The current analytics proc runs a `SELECT SUM(...)` subquery inside the SELECT list for every grouped row, hitting `OrderLines` repeatedly instead of aggregating once. On ~8M lines that pattern scans the same data hundreds of times and is why the page sits at 18s.
>
> **Do instead:** aggregate in one pass (CTE or grouped join as above), ensure `Orders(SchoolId, SeasonYear)` and `OrderLines(OrderId)` are indexed, and pull this into a persisted summary table if we need sub-second loads during August.
>
> Happy to pair on an execution plan review before we ship.
