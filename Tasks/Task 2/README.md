# BookStore Console Application (C# OOP and Advanced Architecture)

## Overview
This is an enterprise-grade, in-memory command-line application built with **.NET 8** to manage bookstore inventory, customer registrations, and purchase transactions. The application is built upon strict Object-Oriented Programming (OOP) principles and a clean separation of concerns, ensuring high maintainability and extensibility.

## Key Architectural Features

* **Polymorphic Domain Model (Requirement #7):** The core inventory system utilizes an `abstract class Book` base. Physical formats (`PaperbackBook`) derive from this base and implement the abstract `GetBookType()` method. Adding future formats (e.g., `EBook`, `AudioBook`) requires zero modification to existing repository or transactional code.
* **Generic In-Memory Repository (Requirement #8):** Data persistence is handled via an `IRepository<T>` interface and an `InMemoryRepository<T>` implementation constrained to `BaseEntity`. This provides universal CRUD operations for Books, Customers, and Purchases without redundant code.
* **Event-Driven Stock Alerts (Requirement #10):** The `StockManager` service utilizes **C# Events and Delegates** (`event Action<Book> OnOutOfStock`). When a transaction reduces a book's inventory to `0`, an decoupled system alert is broadcasted to alert store administrators.
* **Dynamic Lambda Filtering (Requirement #9):** Inventory filtering delegates rule evaluation to the caller using `Func<Book, bool>` delegates, allowing developers to inject arbitrary pricing or category business rules dynamically.
* **Bulletproof Input Validation (Requirement #133):** The `InputValidator` static service wraps all user interactions in safe `TryParse` execution loops, guaranteeing the application will never crash from invalid or unexpected console inputs.

## How to Run the Application

1. Ensure you have the **.NET 8.0 SDK** (or newer) installed.
2. Clone this repository and open a terminal in the project root directory.
3. Run the following command to build and start the application:
   ```bash
   dotnet run --project BookStore.ConsoleApp