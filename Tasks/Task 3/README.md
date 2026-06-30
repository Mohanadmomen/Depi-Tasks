# BookStore Data Layer (Entity Framework Core & LINQ)

## Overview
This repository contains the data access layer for the BookStore system, built using **Entity Framework Core 10** in a C# Class Library. It migrates the temporary, in-memory domain models from Task 2 into a durable **MS SQL Server** relational database using the **Code-First** workflow. 

## Key Technical Implementations

* **Code-First Database Modeling (Requirement #165):** Database tables, keys, and constraints are generated entirely from C# domain models via EF Core Migrations. 
* **Business Rule Enforcement via Fluent API (Requirement #166):** Configured inside `OnModelCreating()`, the database structurally enforces unique customer email addresses, decimal precision (`18,2`), non-negative pricing constraints, and `DeleteBehavior.Restrict` on Categories and Authors to prevent orphan sales records.
* **Historical Pricing Preservation (Requirement #167):** The `PurchaseItem` entity snapshots the exact `UnitPrice` of a book at the exact millisecond of checkout. Future price modifications to the core `Book` table will never alter past financial reporting.
* **$N+1$ Query Elimination (Requirement #184):** Solved lazy-loading performance bottlenecks by applying explicit `.Include(b => b.Category).Include(b => b.Author)` eager loading. This guarantees relational data is pulled in a **single SQL JOIN call** rather than pinging the server per row.
* **Read-Only Optimization (Requirement #185):** All reporting and analytical queries implement `.AsNoTracking()`. This disables EF Core's change tracker, drastically reducing memory consumption and improving query execution times for display-only dashboards.

## Prerequisites & Setup

1. **.NET 10 SDK** installed.
2. **SQL Server** (LocalDB, Developer, or Express Edition) running locally.
3. **EF Core CLI Tools** installed globally:
   ```bash
   dotnet tool install --global dotnet-ef