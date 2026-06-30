# Cineverse Cinema Booking System (ASP.NET Core MVC)

Cineverse is a modern, responsive Cinema Booking Web Application built using **ASP.NET Core 10 MVC**, **Entity Framework Core**, and **ASP.NET Core Identity** with **SQL Server LocalDB**. 

---

## 🚀 Key Features

*   **Public Movie Catalog**: Browse showtimes, view movie genres, details, and seating capacity without logging in.
*   **Authentication & Security**: Fully implemented registration, login, logout, and change password features using ASP.NET Core Identity.
*   **Roles System**: Two roles: `Customer` (default on register) and `Admin` (pre-seeded).
*   **Secure Admin Section**: 
    *   Separate area (`/Admin/...`) protected with `[Authorize(Roles = "Admin")]`.
    *   Manage Movies, Categories, Cinemas, Halls, and Showtimes.
*   **File Uploads**: Admin can upload custom poster images for movies.
*   **Real-time Price Estimation**: Interactive ticket booking page that calculates the total price dynamically on input change using JavaScript (no page reload).
*   **Seating Enforcement**: Rejects bookings if the requested quantity exceeds the remaining seats in the hall.
*   **User Action Feedback**: Displays smooth, custom status toast messages (Success / Error) following actions (booking, cancellations, etc.).
*   **Custom Status Code Pages**: Professional layout fallback for HTTP 404 and 500 errors.
*   **Responsive Dark Vibe Layout**: Fully responsive mobile-friendly design with a dark premium aesthetic, custom Google Font, and glassmorphism.

---

## 🔑 Default Credentials

On first run, the database is automatically created, migrated, and seeded with sample data including an administrative account:

*   **Admin Email**: `admin@cinema.com`
*   **Admin Password**: `AdminPassword123!`

*Note: Newly registered users from the public register page are automatically assigned the `Customer` role.*

---

## 📁 Movie Poster Storage

Uploaded movie posters are stored physically on the server under:
📁 `WebApplication1/WebApplication1/wwwroot/images/posters/`

In the application views, they are referenced relative to the static assets root at:
🌐 `/images/posters/[filename]`

---

## 🛠️ Setup & How to Run

### Prerequisites
*   [.NET SDK 10](https://dotnet.microsoft.com/download)
*   **SQL Server LocalDB** (Standard with Visual Studio Windows installation)

### Steps

1.  **Navigate to project directory**:
    ```bash
    cd WebApplication1/WebApplication1
    ```

2.  **Restore dependencies**:
    ```bash
    dotnet restore
    ```

3.  **Build the application**:
    ```bash
    dotnet build
    ```

4.  **Run the application**:
    ```bash
    dotnet run
    ```
    Once running, open the URL (usually `https://localhost:7198` or `http://localhost:5242`) in your browser to explore Cineverse.
