const API_BASE = 'http://linkvaultapi.runasp.net/api';
const detailsContainer = document.getElementById('bookmark-details-container');
const notesContainer = document.getElementById('nested-notes-container');
let nestedNoteModalInstance;

// Extract ID from URL (e.g., bookmark.html?id=5)
const urlParams = new URLSearchParams(window.location.search);
const bookmarkId = urlParams.get('id');

document.addEventListener('DOMContentLoaded', () => {
    if (!bookmarkId) {
        detailsContainer.innerHTML = '<div class="alert alert-danger">No bookmark ID provided in URL.</div>';
        return;
    }
    nestedNoteModalInstance = new bootstrap.Modal(document.getElementById('nestedNoteModal'));
    loadBookmarkDetails();
    loadNestedNotes();
});

let currentBookmark = null;

async function loadBookmarkDetails() {
    try {
        const res = await fetch(`${API_BASE}/bookmarks/${bookmarkId}`, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (!res.ok) throw new Error("Failed to load details");
        const bm = await res.json();
        currentBookmark = bm;

        const favBtnClass = bm.isFavorite ? 'btn-warning' : 'btn-outline-warning';
        const favIconClass = bm.isFavorite ? 'bi-star-fill' : 'bi-star';
        const archBtnClass = bm.isArchived ? 'btn-success' : 'btn-outline-success';
        const archIconClass = bm.isArchived ? 'bi-archive-fill' : 'bi-archive';

        detailsContainer.innerHTML = `
            <div class="row g-0 align-items-center">
                <div class="col-md-8">
                    <div class="d-flex align-items-center mb-2">
                        <img src="https://www.google.com/s2/favicons?domain=${bm.url}&sz=64" class="me-3 rounded shadow-sm" style="width: 32px; height: 32px;" onerror="this.style.display='none'">
                        <h2 class="fw-bold mb-0 text-break">${bm.title}</h2>
                    </div>
                    <a href="${bm.url}" target="_blank" class="text-primary text-decoration-none fs-5 d-block mb-3 text-truncate">
                        ${bm.url} <i class="bi bi-box-arrow-up-right small ms-1"></i>
                    </a>
                    
                    <div class="d-flex flex-wrap gap-2 mb-3">
                        <span class="badge bg-secondary p-2"><i class="bi bi-folder2-open me-1"></i> Category ID: ${bm.categoryId}</span>
                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-calendar3 me-1"></i> Details View</span>
                    </div>
                </div>
                <div class="col-md-4 text-md-end mt-3 mt-md-0">
                    <div class="btn-group shadow-sm" role="group">
                        <button class="btn ${favBtnClass}" onclick="toggleStatus('favorite')" title="Toggle Favorite">
                            <i class="bi ${favIconClass}"></i> <span class="ms-1">Favorite</span>
                        </button>
                        <button class="btn ${archBtnClass}" onclick="toggleStatus('archive')" title="Toggle Archive">
                            <i class="bi ${archIconClass}"></i> <span class="ms-1">Archive</span>
                        </button>
                    </div>
                </div>
            </div>
            <hr class="my-4 text-muted">
        `;
    } catch (error) {
        detailsContainer.innerHTML = `<div class="alert alert-danger">${error.message}</div>`;
    }
}

window.toggleStatus = async function(fieldToToggle) {
    if (!currentBookmark) return;
    
    const bm = currentBookmark;
    // Calculate new state
    const newIsFavorite = fieldToToggle === 'favorite' ? !bm.isFavorite : bm.isFavorite;
    const newIsArchived = fieldToToggle === 'archive' ? !bm.isArchived : bm.isArchived;

    const payload = {
        id: bm.id || bm.Id || bm.bookmarkId,
        title: bm.title,
        url: bm.url,
        categoryId: bm.categoryId,
        isFavorite: newIsFavorite,
        isArchived: newIsArchived
    };

    try {
        // Optimistic update
        if (fieldToToggle === 'favorite') bm.isFavorite = newIsFavorite;
        if (fieldToToggle === 'archive') bm.isArchived = newIsArchived;
        
        // Show loading state on button if desired, but for now just refresh acts as feedback
        
        const response = await fetch(`${API_BASE}/bookmarks/${bookmarkId}`, {
            method: 'PUT', 
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` },
            body: JSON.stringify(payload) 
        });

        if (!response.ok) throw new Error(`Failed to update status`);
        loadBookmarkDetails(); // Reload UI to match server state
    } catch (error) {
        alert(error.message);
        loadBookmarkDetails(); // Revert UI
    }
};

// --- Nested Notes Logic ---
// IMPORTANT: Check Swagger for the exact endpoint for nested notes!
const NESTED_NOTES_API = `${API_BASE}/Bookmarks/${bookmarkId}/Notes`;

async function loadNestedNotes() {
    notesContainer.innerHTML = '<p class="text-center text-muted p-3">Loading notes...</p>';
    try {
        const res = await fetch(NESTED_NOTES_API, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (!res.ok) throw new Error("Failed to load notes");
        const notes = await res.json();

        if (notes.length === 0) {
            notesContainer.innerHTML = '<div class="list-group-item text-center text-muted py-4">No notes attached to this bookmark yet.</div>';
            return;
        }

        let html = '';
        notes.forEach(n => {
            const noteId = n.id || n.Id;
            html += `
                <div class="list-group-item list-group-item-action d-flex justify-content-between align-items-center">
                    <div>${n.content}</div>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteNestedNote('${noteId}')"><i class="bi bi-trash"></i></button>
                </div>
            `;
        });
        notesContainer.innerHTML = html;
    } catch (error) {
        notesContainer.innerHTML = `<div class="list-group-item text-danger">${error.message}</div>`;
    }
}

document.getElementById('add-note-btn').addEventListener('click', () => {
    document.getElementById('nested-note-content').value = '';
    nestedNoteModalInstance.show();
});

document.getElementById('save-nested-note-btn').addEventListener('click', async () => {
    const content = document.getElementById('nested-note-content').value.trim();
    if (!content) return;

    try {
        const res = await fetch(NESTED_NOTES_API, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` },
            body: JSON.stringify({ content: content }) // Check Swagger for DTO shape
        });
        if (!res.ok) throw new Error("Failed to add note");
        
        nestedNoteModalInstance.hide();
        loadNestedNotes();
    } catch (error) {
        alert(error.message);
    }
});

window.deleteNestedNote = async function(noteId) {
    if (!confirm("Delete this note?")) return;
    try {
        const res = await fetch(`${NESTED_NOTES_API}/${noteId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (!res.ok && res.status !== 204) throw new Error("Failed to delete note");
        loadNestedNotes();
    } catch (error) {
        alert(error.message);
    }
};