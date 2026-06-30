# 🔒 Link Vault - Frontend Dashboard

Welcome to the **Link Vault Frontend**, a responsive and modern web-based dashboard designed to help users store, organize, and manage bookmarks, categories, and notes. The project integrates seamlessly with the backend RESTful API.

Developed as a single-page style multi-view application using **Vanilla HTML5, modern ES6+ JavaScript, CSS3, and Bootstrap 5.3**, it features custom styling, JWT authorization, automated client route guards, and a dark/light mode toggle.

---

## 🚀 Key Features

*   **🔐 JWT Authentication & Guard**:
    *   Secure **Sign In** and **Registration** forms.
    *   Client-side routing guard (`app.js`) that automatically checks for authentication tokens and redirects unauthenticated users to the login screen.
    *   Displays user information (email address) decoded directly from the JWT claims in the navigation bar.
*   **📂 Category Management (CRUD)**:
    *   Organize bookmarks and notes under custom categories.
    *   Create, view, update, and delete categories with inline modal forms.
*   **🔖 Advanced Bookmarks Vault**:
    *   View bookmarks inside an interactive data table.
    *   Filter bookmarks by **Search term**, **Category selection**, **Favorite status**, or **Archived status** in real time.
    *   Quick toggle for pinning items to favorites (⭐) or archiving (📥) them.
    *   Automatic favicon loading using Google favicon API services.
*   **📄 Bookmark Detail & Nested Notes**:
    *   Dedicated view for inspecting specific bookmark records (`bookmark.html`).
    *   Create and manage short notes attached specifically to individual bookmarks.
*   **📝 Dynamic Note Board**:
    *   Create, edit, and delete notes.
    *   Support for pinning notes to the top of the list.
    *   Live filtering of notes by category and text search query.
*   **🌓 Native Dark Mode Support**:
    *   Theme toggle built on Bootstrap 5's styling.
    *   Persists user choice locally via `localStorage` for visual consistency across sessions.

---

## 🛠️ Technology Stack & CDNs

*   **Structure**: [HTML5](https://developer.mozilla.org/en-US/docs/Web/HTML)
*   **Styles**: [Bootstrap 5.3.0](https://getbootstrap.com/) & [Bootstrap Icons 1.10.5](https://icons.getbootstrap.com/)
*   **Logic**: Pure modern JavaScript ([ES6+](https://developer.mozilla.org/en-US/docs/Web/JavaScript))
*   **HTTP Clients**: Fetch API using asynchronous `async/await` patterns.

---

## 📂 File Directory Structure

Here is an overview of the frontend files and their respective responsibilities:

```txt
Link Vault Frontend Project/
├── Index.html           # Dashboard home view. Shows Category manager.
├── style.css            # Global styling, customization, and theme variables.
├── app.js               # Common script: auth checking, theme toggles, and token decoding.
│
├── login.html           # Authentication portal (Sign In & Sign Up interfaces).
├── login.css            # Styles specific to the registration and login forms.
├── auth.js              # Registers event listeners for forms, connects to auth endpoints.
│
├── categories.js        # Controller logic for listing, editing, creating, and deleting categories.
│
├── bookmarks.html       # Bookmarks gallery page with sorting and filtering options.
├── bookmarks.js         # Bookmark CRUD operations, live client filters, and favoriting.
│
├── bookmark.html        # Detailed view for a singular bookmark.
├── bookmark.js          # Detail viewer and nested notes manager logic.
│
├── notes.html           # Notes manager interface.
└── notes.js             # Manage markdown/plain text notes, pinning, and categories.
```

---

## 🔌 API Configuration & Endpoints

All fetch operations connect to the following centralized REST backend:
`http://linkvaultapi.runasp.net/api`

### Integration Matrix

| Feature | HTTP Method | API Path | Description |
| :--- | :--- | :--- | :--- |
| **Auth** | `POST` | `/Auth/Register` | Register a new user |
| **Auth** | `POST` | `/Auth/Login` | Login user and retrieve JWT |
| **Categories** | `GET` | `/Categories` | Retrieve all user categories |
| **Categories** | `POST` | `/Categories` | Create a new category |
| **Categories** | `PUT` | `/Categories/{id}` | Update category details |
| **Categories** | `DELETE` | `/Categories/{id}` | Delete category |
| **Bookmarks** | `GET` | `/bookmarks` | Retrieve bookmarks (supports search/filtering) |
| **Bookmarks** | `POST` | `/bookmarks` | Create bookmark record |
| **Bookmarks** | `PUT` | `/bookmarks/{id}` | Update bookmark (incl. favorite/archive states) |
| **Bookmarks** | `DELETE` | `/bookmarks/{id}` | Delete bookmark |
| **Nested Notes**| `GET` | `/Bookmarks/{id}/Notes` | List notes attached to a bookmark |
| **Nested Notes**| `POST` | `/Bookmarks/{id}/Notes` | Attach a note to a bookmark |
| **Nested Notes**| `DELETE` | `/Bookmarks/{id}/Notes/{noteId}`| Delete note attached to a bookmark |
| **Notes** | `GET` | `/Notes` | Get all user notes |
| **Notes** | `POST` | `/Notes` | Create note |
| **Notes** | `PUT` | `/Notes/{id}` | Update note contents |
| **Notes** | `DELETE` | `/Notes/{id}` | Delete note |
| **Notes** | `PATCH` | `/Notes/{id}/pin` | Toggle pin status of a note |

---

## 💻 How to Run Locally

Because the frontend is built entirely of static files, it does not require a compilation step. However, due to browser security settings around local file operations (CORS/origin rules) and relative paths, it is **strongly recommended** to serve it through a web server rather than opening `.html` files directly.

### Option 1: Visual Studio Code (Recommended)
1. Install the **Live Server** extension.
2. Open the `Link Vault Frontend Project` folder.
3. Click **Go Live** on the status bar (usually at the bottom right).

### Option 2: Node.js (npx)
If you have Node.js installed, open your terminal inside the project directory and run:
```bash
npx serve .
```

### Option 3: Python
If you have Python installed, run one of the following commands in the directory:
```bash
# Python 3
python -m http.server 8000

# Python 2
python -m SimpleHTTPServer 8000
```
Then navigate to `http://localhost:8000` in your web browser.

---

## 🔒 Security & Session Storage

1. **Authorization**: When a user registers or logs in, the API returns a JWT token. This token is securely stored in `localStorage` as `jwt_token`.
2. **Session Expiry & Handling**: Standard `fetch` requests include the token in the headers as:
   ```json
   {
     "Authorization": "Bearer <YOUR_JWT_TOKEN>"
   }
   ```
   If the API returns a `401 Unauthorized` status (indicating an expired or invalid token), the frontend clears the local token storage and automatically redirects the browser back to `login.html`.
