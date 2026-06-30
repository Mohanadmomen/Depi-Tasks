const API_BASE = 'http://linkvaultapi.runasp.net/api';
const NOTES_API = `${API_BASE}/Notes`;
const CATEGORIES_API = `${API_BASE}/categories`;

const dataContainer = document.getElementById('data-container');
let noteModalInstance;
let categoryMap = {}; 
let currentNotes = []; 

document.addEventListener('DOMContentLoaded', async () => {
    noteModalInstance = new bootstrap.Modal(document.getElementById('noteModal'));
    
    // Auth check might be in auth.js, but let's double check or assume auth.js runs first
    // If not, we can add a check here to be safe.
    if (!localStorage.getItem('jwt_token')) {
        window.location.href = 'login.html';
        return;
    }

    await loadCategoriesForDropdowns();
    loadNotes();
    
    // Event Listeners
    document.getElementById('create-note-btn').addEventListener('click', () => openEditModal());
    document.getElementById('note-form').addEventListener('submit', handleFormSubmit);
    
    // Use client-side filtering (filterAndRenderNotes) instead of reloading from API
    document.getElementById('filter-category').addEventListener('change', filterAndRenderNotes);
    document.getElementById('filter-search').addEventListener('input', filterAndRenderNotes);
    
    document.getElementById('clear-filters-btn').addEventListener('click', () => {
        document.getElementById('filter-category').value = '';
        document.getElementById('filter-search').value = '';
        filterAndRenderNotes();
    });
});

function showAlert(message, type = 'success') {
    const alertContainer = document.getElementById('alert-container');
    const alertId = `alert-${Date.now()}`;
    alertContainer.innerHTML += `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show shadow" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    setTimeout(() => {
        const el = document.getElementById(alertId);
        if (el) new bootstrap.Alert(el).close();
    }, 4000);
}

function debounce(func, wait) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

// --- Fetch Categories ---
async function loadCategoriesForDropdowns() {
    try {
        const res = await fetch(CATEGORIES_API, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (!res.ok) throw new Error('Failed to load categories');
        const categories = await res.json();
        
        const filterSelect = document.getElementById('filter-category');
        const formSelect = document.getElementById('note-category');
        
        // Clear existing options (keep default)
        filterSelect.innerHTML = '<option value="">All Categories</option>';
        formSelect.innerHTML = '<option value="" disabled selected>Select a category...</option>';

        categories.forEach(cat => {
            categoryMap[cat.id] = cat.categoryName; 
            filterSelect.innerHTML += `<option value="${cat.id}">${cat.categoryName}</option>`;
            formSelect.innerHTML += `<option value="${cat.id}">${cat.categoryName}</option>`;
        });
    } catch (error) {
        console.error("Error loading categories:", error);
        showAlert("Failed to load categories. Please refresh.", "danger");
    }
}

// --- Fetch & Render Notes ---
async function loadNotes() {
    // Only show spinner if we don't have data yet (initial load)
    if (currentNotes.length === 0) {
        dataContainer.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div><p class="mt-2 text-muted">Loading notes...</p></div>';
    }
    
    // Always fetch ALL notes to support robust client-side filtering
    // (Since API filtering support is uncertain or inconsistent)
    const url = NOTES_API;

    try {
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        
        if (response.status === 401) {
            localStorage.removeItem('jwt_token');
            window.location.href = 'login.html';
            return;
        }
        if (!response.ok) throw new Error('Failed to fetch notes');

        currentNotes = await response.json(); 
        
        // Apply client-side filters immediately after fetching
        filterAndRenderNotes();
    } catch (error) {
        dataContainer.innerHTML = `<p class="text-center text-danger my-4">${error.message}</p>`;
        showAlert(error.message, 'danger');
    }
}

// --- Client-Side Filtering ---
function filterAndRenderNotes() {
    const categoryId = document.getElementById('filter-category').value;
    const search = document.getElementById('filter-search').value.trim().toLowerCase();

    let filtered = currentNotes;

    // Filter by Category
    if (categoryId) {
        filtered = filtered.filter(note => {
            // Check implicit ID property
            const noteCatId = note.categoryId || note.CategoryId;
            if (noteCatId && String(noteCatId) === String(categoryId)) return true;

            // Check Category Name (mapped from ID)
            const noteCatName = note.categoryName || note.CategoryName;
            const targetName = categoryMap[categoryId];
            if (noteCatName && targetName && noteCatName === targetName) return true;

            return false;
        });
    }

    // Filter by Search (Title or Content)
    if (search) {
        filtered = filtered.filter(note => {
            const title = (note.title || note.Title || '').toLowerCase();
            const content = (note.content || note.Content || '').toLowerCase();
            return title.includes(search) || content.includes(search);
        });
    }

    renderTable(filtered);
}

function renderTable(notes) {
    if (!notes || notes.length === 0) {
        dataContainer.innerHTML = `<div class="text-center py-5"><i class="bi bi-journal-x display-4 text-muted"></i><p class="mt-3 text-muted">No notes found.</p></div>`;
        return;
    }

    let html = `
    <table class="table table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th style="width: 50px;">Pin</th>
                <th>Title</th>
                <th>Content (Preview)</th>
                <th>Category</th>
                <th class="text-end">Actions</th>
            </tr>
        </thead>
        <tbody>`;

    notes.forEach(note => {
        // Robust ID check
        const safeId = note.id || note.Id || note.noteId || note.NoteId; 
        
        // Robust Category ID check
        // API returns "categoryName" directly, but no categoryId. 
        // We will use categoryName for display.
        const catName = note.categoryName || categoryMap[note.categoryId] || 'Unknown'; 
        
        // API returns "pinned" (lowercase) based on user feedback
        let isPinned = note.pinned;
        if (isPinned === undefined) isPinned = note.isPinned;
        if (isPinned === undefined) isPinned = note.IsPinned;
        if (isPinned === undefined) isPinned = note.Pinned;
        
        // Default to false if still undefined
        if (isPinned === undefined) isPinned = false;

        const pinClass = isPinned ? 'bi-pin-fill pinned-active' : 'bi-pin';
        
        // Robust Content check
        const content = note.content || note.Content || '';
        const title = note.title || note.Title || 'Untitled';

        // DEBUG: Uncomment to see structure in console
        // console.log("Note ID:", safeId, "Pinned:", isPinned, "CatID:", catId, "Raw:", note);

        // Truncate content
        const truncatedContent = content.length > 50 ? content.substring(0, 50) + '...' : content;

        html += `
            <tr>
                <td class="text-center">
                    <i class="bi ${pinClass}" style="cursor: pointer; font-size: 1.2rem;" onclick="togglePin('${safeId}')" title="Toggle Pin"></i>
                    <!-- DEBUG: Show state -->
                    <!-- <br><small style="font-size:8px">${isPinned}</small> -->
                </td>
                <td class="fw-bold text-primary">
                    ${title}
                </td>
                <td class="text-muted small">${truncatedContent}</td>
                <td><span class="badge bg-secondary">${catName}</span></td>
                <td class="text-end text-nowrap">
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="openEditModal('${safeId}')"><i class="bi bi-pencil"></i></button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteNote('${safeId}')"><i class="bi bi-trash"></i></button>
                </td>
            </tr>`;
    });
    dataContainer.innerHTML = html + `</tbody></table>`;
}

// --- Create & Edit Logic ---
window.openEditModal = function(id = null) {
    const modalTitle = document.getElementById('noteModalTitle');
    const idInput = document.getElementById('note-id');
    const titleInput = document.getElementById('note-title');
    const contentInput = document.getElementById('note-content');
    const categoryInput = document.getElementById('note-category');

    // Reset Form
    document.getElementById('note-form').reset();

    if (id) {
        // Edit Mode
        // Robust find with ID check
        const note = currentNotes.find(n => String(n.id || n.Id || n.noteId || n.NoteId) === String(id));
        
        if (!note) {
            showAlert("Error: Note not found locally.", "danger");
            return;
        }
        modalTitle.textContent = "Edit Note";
        idInput.value = id;
        titleInput.value = note.title || note.Title;
        contentInput.value = note.content || note.Content;
        
        // Resolve Category ID for the dropdown
        // If API doesn't return categoryId, we must reverse-lookup by name
        let catId = note.categoryId || note.CategoryId;
        
        if (!catId && note.categoryName) {
            // Reverse lookup: Find ID where name matches
            const foundId = Object.keys(categoryMap).find(key => categoryMap[key] === note.categoryName);
            if (foundId) catId = foundId;
        }

        if (catId) {
             categoryInput.value = catId;
        }
    } else {
        // Create Mode
        modalTitle.textContent = "New Note";
        idInput.value = "";
    }

    noteModalInstance.show();
};

async function handleFormSubmit(e) {
    e.preventDefault();
    
    const id = document.getElementById('note-id').value;
    const title = document.getElementById('note-title').value.trim();
    const content = document.getElementById('note-content').value.trim();
    const categoryId = document.getElementById('note-category').value;

    if (!categoryId) {
        showAlert('Please select a valid category.', 'warning');
        return;
    }

    const payload = { 
        title, 
        content, 
        categoryId: parseInt(categoryId, 10) // Ensure it's a number
    };
    
    // If editing, include ID in payload if API requires it, usually params handle it
    if (id) payload.id = id; 

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${NOTES_API}/${id}` : NOTES_API;

    try {
        const response = await fetch(url, {
            method: method,
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errData = await response.json().catch(() => ({}));
            throw new Error(errData.message || 'Failed to save note');
        }
        
        showAlert(`Note ${id ? 'updated' : 'created'} successfully!`);
        noteModalInstance.hide();
        loadNotes();
    } catch (error) {
        showAlert(error.message, 'danger');
    }
}

// --- Delete Logic ---
window.deleteNote = async function(id) {
    if (!confirm('Are you sure you want to delete this note? This action cannot be undone.')) return;

    try {
        const response = await fetch(`${NOTES_API}/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });

        if (!response.ok) throw new Error('Failed to delete note');
        
        showAlert('Note deleted successfully.');
        loadNotes();
    } catch (error) {
        showAlert(error.message, 'danger');
    }
};

// --- Toggle Pin Logic ---
window.togglePin = async function(id) {
    // Robust find
    const note = currentNotes.find(n => String(n.id || n.Id || n.noteId || n.NoteId) === String(id));
    let currentIsPinned = note.isPinned;
    if (currentIsPinned === undefined) currentIsPinned = note.IsPinned;
    if (currentIsPinned === undefined) currentIsPinned = note.pinned;
    if (currentIsPinned === undefined) currentIsPinned = note.Pinned;
    if (currentIsPinned === undefined) currentIsPinned = false;

    const newPinnedStatus = !currentIsPinned;

    try {
        const response = await fetch(`${NOTES_API}/${id}/pin`, {
            method: 'PATCH',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
            },
            body: JSON.stringify({}) 
        });

        if (!response.ok) throw new Error('Failed to update pin status');
        
        // Optimistic update locally - update ALL possible casings to be safe
        note.pinned = newPinnedStatus;
        note.isPinned = newPinnedStatus;
        note.IsPinned = newPinnedStatus;
        
        renderTable(currentNotes);
        showAlert(`Note ${newPinnedStatus ? 'pinned' : 'unpinned'}!`);
    } catch (error) {
        console.error(error);
        showAlert("Failed to toggle pin. Please try again.", 'danger');
        loadNotes(); // Revert on error
    }
};