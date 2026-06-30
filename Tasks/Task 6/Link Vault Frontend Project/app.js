// --- 1. Authentication Guard ---
// If any page is accessed without a token, redirect to login immediately [cite: 26]
const token = localStorage.getItem('jwt_token');
if (!token) {
    window.location.href = 'login.html';
}

// --- 2. Decode JWT for Email ---
// Decode it from the JWT - don't call the API for it [cite: 28]
function decodeJWT(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payloadObj = JSON.parse(jsonPayload);
        // Extract the specific ASP.NET email claim [cite: 85]
        return payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    } catch (e) {
        console.error("Failed to decode JWT", e);
        return "Unknown User";
    }
}

// Display the email in the navbar [cite: 32]
document.addEventListener('DOMContentLoaded', () => {
    const userEmailSpan = document.getElementById('user-email');
    if (userEmailSpan && token) {
        userEmailSpan.textContent = decodeJWT(token);
    }
});

// --- 3. Logout Logic ---
// User can logout (clear token + redirect) [cite: 27]
document.addEventListener('DOMContentLoaded', () => {
    const logoutBtn = document.getElementById('logout-btn');

    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('jwt_token');
            window.location.href = 'login.html';
        });
    }
});

// --- 4. Dark Mode Toggle ---
// Switch between light and dark theme 
document.addEventListener('DOMContentLoaded', () => {
    const themeToggleBtn = document.getElementById('theme-toggle');
    const htmlElement = document.documentElement;
    
    // Check local storage for saved theme, default to light
    const savedTheme = localStorage.getItem('theme') || 'light';
    htmlElement.setAttribute('data-bs-theme', savedTheme);
    updateThemeIcon(savedTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', () => {
            const currentTheme = htmlElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';
            
            htmlElement.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeIcon(newTheme);
        });
    }

    function updateThemeIcon(theme) {
        if (!themeToggleBtn) return;
        if (theme === 'dark') {
            themeToggleBtn.classList.remove('bi-moon-stars');
            themeToggleBtn.classList.add('bi-sun');
        } else {
            themeToggleBtn.classList.remove('bi-sun');
            themeToggleBtn.classList.add('bi-moon-stars');
        }
    }
});