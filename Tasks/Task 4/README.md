# BookStore Web API

A robust, secure, and fully featured ASP.NET Core Web API for an online bookstore.

---

## ✨ Implemented Features

This project contains a complete implementation of the BookStore Web API incorporating the following:
* **JWT Authentication & Registration**: Secure, database-backed customer registration and login using ASP.NET Core `PasswordHasher` for deterministic salted password verification.
* **Books Management**: Endpoints supporting book retrieval with server-side keyword search, filtering (by author, category, and price range), pagination, and soft-delete capabilities.
* **Categories & Authors CRUD**: Full CRUD endpoints for managing categories and authors.
* **Transactional Orders Engine**: Transaction-backed checkout workflow that validates book availability, deducts inventory stock, and logs order line-items.
* **Admin Dashboard**: Live statistical summary displaying aggregated metrics like active books, categories, order count, and total revenue.
* **Data Transfer Objects (DTOs)**: Clean separation of database entities from API clients to protect internal schema design.
* **Global Exception Handling & Logging**: Diagnostic middleware intercepting server exceptions, paired with audit logging for logins, orders, and system errors.
* **CORS & Swagger Documentation**: Configured CORS policies for local client requests, plus Swagger UI supporting Bearer JWT token authorization.

---

## 🚀 How to Run

1. **Prerequisites**: Ensure you have SQL Server running locally and the .NET SDK (version 10.0 or compatible) installed.
2. **Apply Migrations**: Apply the EF Core database migrations by running the following command in the project directory (`BookStore.Data`):
   ```powershell
   dotnet ef database update
   ```
3. **Run the API**: Start the application by pressing `F5` in Visual Studio or running:
   ```powershell
   dotnet run
   ```
4. **Access the API**: The API runs locally, and the Swagger documentation UI will open automatically at `/swagger`.

---

## 🔒 Authentication & Roles

This API implements JWT-based authentication with two distinct roles:
* **Admin**: Authorized to manage books, categories, authors, and view all system-wide orders.
* **Customer**: Authorized to browse books, place orders, and view their own order history.

### Seeded Test Accounts
The database is automatically seeded with a default administrator account upon creation, meaning the first admin is pre-registered out-of-the-box:
* **Admin Account**:
  * **Email**: `admin@test.com`
  * **Password**: `password`
* **Customer Account**:
  * **Email**: `ahmed@test.com`
  * **Password**: `password`

---

## 👑 How to Register/Promote the First Admin

To maintain security, the public registration endpoint (`/api/Auth/register`) defaults all new accounts to the `Customer` role. To set up or promote a custom Administrator, use one of the following methods:

### Method 1: Use the Seeded Admin (Recommended)
Simply log in with the pre-seeded admin account credentials:
* **Email**: `admin@test.com`
* **Password**: `password`

### Method 2: Elevate a Registered User via SQL
If you register a new account (e.g., `user@example.com`) through the API and want to elevate them to the `Admin` role:
1. Connect to your local SQL Server instance using **SSMS (SQL Server Management Studio)** or VS database tools.
2. Open a query window targeting the `BookStoreEFCoreDB` database.
3. Run the following SQL query to update the user's role:
   ```sql
   UPDATE Customers 
   SET Role = 'Admin' 
   WHERE Email = 'user@example.com';
   ```

---

## 🧪 How to Test

### 1. Register a New Customer
Send a `POST` request to `/api/Auth/register`:
```bash
curl -X 'POST' \
  'https://localhost:<PORT>/api/Auth/register' \
  -H 'Content-Type: application/json' \
  -d '{
  "fullName": "John Doe",
  "email": "john@example.com",
  "city": "Cairo",
  "password": "SecurePassword123"
}'
```

### 2. Login
Send a `POST` request to `/api/Auth/login` using your registered email and password to receive a JWT Token:
```bash
curl -X 'POST' \
  'https://localhost:<PORT>/api/Auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "john@example.com",
  "password": "SecurePassword123"
}'
```

### 3. Authorized Requests
1. Copy the `token` from the login response.
2. Open Swagger UI at `/swagger`.
3. Click the **Authorize 🔒** button at the top-right of the page.
4. Paste the JWT token in the text field (without prefixing it with 'Bearer') and click **Authorize**.
5. All subsequent requests executed through Swagger will include the authentication token in the headers.

---

## 📂 Core Endpoints Map

* **Auth**:
  * `POST /api/Auth/register` — Register a customer account
  * `POST /api/Auth/login` — Sign in and receive a JWT token
* **Books**:
  * `GET /api/Books` — Search, filter (by Category, Author, Price range), and page books (Public)
  * `GET /api/Books/{id}` — Get detailed book profile (Public)
  * `POST /api/Books` — Create a book (Admin only)
  * `PUT /api/Books/{id}` — Edit book details (Admin only)
  * `DELETE /api/Books/{id}` — Soft delete a book (Admin only)
* **Categories (Admin Only)**:
  * `GET`, `POST`, `PUT`, `DELETE` to `/api/Categories`
* **Authors (Admin Only)**:
  * `GET`, `POST`, `PUT`, `DELETE` to `/api/Authors`
* **Orders**:
  * `POST /api/Orders` — Place order with stock checking and deduction (Customer only)
  * `GET /api/Orders` — View own order history (Customer) or view all orders (Admin)
* **Admin Dashboard (Admin Only)**:
  * `GET /api/admin/dashboard/stats` — Summary of books, categories, orders, and total revenue
  * `GET /api/admin/dashboard/orders` — List of all system orders