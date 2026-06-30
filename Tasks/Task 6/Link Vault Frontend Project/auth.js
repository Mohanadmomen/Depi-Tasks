// Base API URL (Derived from the provided Swagger link)
const API_BASE = 'http://linkvaultapi.runasp.net/api';

// DOM Elements
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const formTitle = document.getElementById('form-title');
const errorAlert = document.getElementById('auth-error');

// --- Initialization ---
// If the user already has a token, redirect away from the login page
window.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
        window.location.href = 'index.html'; 
    }
});

// --- UI Toggles ---
document.getElementById('show-register').addEventListener('click', (e) => {
    e.preventDefault();
    loginForm.classList.add('hidden');
    registerForm.classList.remove('hidden');
    formTitle.textContent = 'Register';
    errorAlert.classList.add('hidden');
});

document.getElementById('show-login').addEventListener('click', (e) => {
    e.preventDefault();
    registerForm.classList.add('hidden');
    loginForm.classList.remove('hidden');
    formTitle.textContent = 'Login';
    errorAlert.classList.add('hidden');
});

// --- API Calls ---
async function handleAuth(endpoint, payload, buttonId) {
    const button = document.getElementById(buttonId);
    button.disabled = true;
    errorAlert.classList.add('hidden');

    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            // Check if API returns a specific error message format
            const errorData = await response.json().catch(() => null);
            throw new Error(errorData?.message || 'Authentication failed. Please check your details.');
        }

        const data = await response.json();
        
        // Assuming the API returns an object with a 'token' property
        if (data.token) {
            localStorage.setItem('jwt_token', data.token);
            window.location.href = 'index.html'; // Redirect to main page
        } else {
            throw new Error('Token not received from server.');
        }

    } catch (error) {
        errorAlert.textContent = error.message;
        errorAlert.classList.remove('hidden');
    } finally {
        button.disabled = false;
    }
}

// --- Event Listeners ---
loginForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const payload = {
        email: document.getElementById('login-email').value,
        password: document.getElementById('login-password').value
    };
    // Note: Verify the exact endpoint path in your Swagger docs (e.g., /Auth/Login)
    handleAuth('/Auth/Login', payload, 'login-btn');
});

registerForm.addEventListener('submit', (e) => {
    e.preventDefault();
    
    // Grab the full name string and remove extra spaces at the ends
    const fullNameInput = document.getElementById('reg-name').value.trim();
    
    // Split the name by spaces
    const nameParts = fullNameInput.split(' ');
    
    // Assign the first word to FirstName, and the rest to LastName
    const firstName = nameParts[0];
    // If they only type one word, we use a fallback so the API doesn't crash
    const lastName = nameParts.slice(1).join(' ') || 'User'; 

    // Match the exact casing the ASP.NET API is expecting
    const payload = {
        FirstName: firstName,
        LastName: lastName,
        Email: document.getElementById('reg-email').value,
        Password: document.getElementById('reg-password').value
    };
    
    handleAuth('/Auth/Register', payload, 'register-btn');
});

// --- Utility: Decode JWT ---
// You will use this in Phase 2 for the Navbar
function decodeJWT(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payloadObj = JSON.parse(jsonPayload);
        // ASP.NET stores email in this specific claim URL
        return payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    } catch (e) {
        console.error("Failed to decode JWT", e);
        return null;
    }
}