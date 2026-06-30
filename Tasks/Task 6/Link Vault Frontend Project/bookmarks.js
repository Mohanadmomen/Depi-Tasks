const API_BASE = 'http://linkvaultapi.runasp.net/api';
const GET_BOOKMARKS_API = `${API_BASE}/bookmarks`; // Plural for fetching list
const SINGLE_BOOKMARK_API = `${API_BASE}/bookmarks`; // Singular for Create/Edit/Toggle/Delete

const dataContainer = document.getElementById('data-container');
let bookmarkModalInstance;
let categoryMap = {}; 
let currentBookmarks = []; // Store data in memory to easily rebuild the DTO

document.addEventListener('DOMContentLoaded', async () => {
    bookmarkModalInstance = new bootstrap.Modal(document.getElementById('bookmarkModal'));
    await loadCategoriesForDropdowns();
    loadBookmarks();

    // Event Listeners
    document.getElementById('create-new-btn').addEventListener('click', openCreateModal);
    document.getElementById('save-bookmark-btn').addEventListener('click', saveBookmark);
});

function showAlert(message, type = 'success') {
    const alertContainer = document.getElementById('alert-container');
    alertContainer.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show shadow" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    setTimeout(() => {
        const alertNode = alertContainer.querySelector('.alert');
        if (alertNode) new bootstrap.Alert(alertNode).close();
    }, 4000);
}

// --- Fetch Categories for Dropdowns ---
async function loadCategoriesForDropdowns() {
    try {
        const res = await fetch(`${API_BASE}/Categories`, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (!res.ok) throw new Error('Failed to load categories');
        const categories = await res.json();
        
        const filterSelect = document.getElementById('filter-category');
        const formSelect = document.getElementById('bookmark-category');
        
        // Reset dropdowns to ensure no duplicates if called multiple times
        filterSelect.innerHTML = '<option value="">All Categories</option>';
        formSelect.innerHTML = '<option value="" disabled selected>Select a category...</option>';

        categories.forEach(cat => {
            // Robust property access
            const id = cat.id || cat.Id || cat.categoryId;
            const name = cat.categoryName || cat.CategoryName || cat.name || cat.Name;
            
            if (id && name) {
                categoryMap[id] = name; 
                filterSelect.innerHTML += `<option value="${id}">${name}</option>`;
                formSelect.innerHTML += `<option value="${id}">${name}</option>`;
            }
        });
    } catch (error) {
        console.error("Error loading categories:", error);
        showAlert("Failed to load categories.", "danger");
    }
}

// --- Fetch & Render Bookmarks ---
async function loadBookmarks() {
    dataContainer.innerHTML = '<p class="text-center text-muted my-4">Loading bookmarks...</p>';
    
    const search = document.getElementById('filter-search').value.trim();
    const categoryId = document.getElementById('filter-category').value;
    const isFav = document.getElementById('filter-favorites').checked;
    const isArch = document.getElementById('filter-archived').checked;

    let url = `${GET_BOOKMARKS_API}?`;
    if (search) url += `search=${encodeURIComponent(search)}&`;
    if (categoryId) url += `categoryId=${categoryId}&`;
    if (isFav) url += `isFavorite=true&`;
    if (isArch) url += `isArchived=true&`;

    try {
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        
        if (response.status === 401) {
            localStorage.removeItem('jwt_token');
            window.location.href = 'login.html';
            return;
        }
        if (!response.ok) throw new Error('Failed to fetch bookmarks');

        currentBookmarks = await response.json(); 
        renderTable(currentBookmarks);
    } catch (error) {
        dataContainer.innerHTML = `<p class="text-center text-danger my-4">${error.message}</p>`;
    }
}

// Filter Event Listeners
const searchInput = document.getElementById('filter-search');
const categorySelect = document.getElementById('filter-category');
const favCheckbox = document.getElementById('filter-favorites');
const archCheckbox = document.getElementById('filter-archived');

// Live filtering
searchInput.addEventListener('input', debounce(loadBookmarks, 500));
categorySelect.addEventListener('change', loadBookmarks);
favCheckbox.addEventListener('change', loadBookmarks);
archCheckbox.addEventListener('change', loadBookmarks);

document.getElementById('filter-form').addEventListener('submit', (e) => {
    e.preventDefault();
    loadBookmarks();
});

document.getElementById('clear-filters-btn').addEventListener('click', () => {
    document.getElementById('filter-form').reset();
    loadBookmarks();
});

function debounce(func, wait) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

// "View Archived" Button Handler
document.getElementById('view-archived-btn').addEventListener('click', () => {
    // Reset other filters to avoid confusion, or keep them?
    // Let's reset search and category to focus on "Archived"
    document.getElementById('filter-search').value = '';
    document.getElementById('filter-category').value = '';
    
    // Toggle the checkbox
    const archivedCheckbox = document.getElementById('filter-archived');
    archivedCheckbox.checked = !archivedCheckbox.checked;

    // Update button text/style optionally
    const btn = document.getElementById('view-archived-btn');
    if (archivedCheckbox.checked) {
        btn.classList.remove('btn-outline-secondary');
        btn.classList.add('btn-secondary');
        btn.innerHTML = '<i class="bi bi-archive-fill"></i> Hide Archived';
    } else {
        btn.classList.remove('btn-secondary');
        btn.classList.add('btn-outline-secondary');
        btn.innerHTML = '<i class="bi bi-archive"></i> View Archived';
    }

    loadBookmarks();
});

// --- Render Table (Bulletproof ID Fix) ---
function renderTable(bookmarks) {
    if (!bookmarks || bookmarks.length === 0) {
        dataContainer.innerHTML = `<div class="text-center py-5"><i class="bi bi-bookmark-x display-4 text-muted"></i><p class="mt-3 text-muted">No bookmarks found.</p></div>`;
        return;
    }

    let html = `<table class="table table-hover align-middle"><thead class="table-light"><tr><th>Status</th><th>Title & URL</th><th>Category</th><th class="text-end">Actions</th></tr></thead><tbody>`;

    bookmarks.forEach(bm => {
        // Look for any possible ID format C# might have sent
        const safeId = bm.id || bm.Id || bm.bookmarkId; 
        const catName = categoryMap[bm.categoryId || bm.CategoryId] || 'Unknown'; 
        const favClass = bm.isFavorite ? 'bi-star-fill favorite-active' : 'bi-star';
        const archClass = bm.isArchived ? 'bi-archive-fill archive-active' : 'bi-archive';

        html += `
            <tr>
                <td style="width: 80px;">
                    <i class="bi ${favClass} action-icon me-2" style="font-size: 1.2rem; cursor: pointer;" onclick="toggleStatus('${safeId}', 'favorite')"></i>
                    <i class="bi ${archClass} action-icon" style="font-size: 1.2rem; cursor: pointer;" onclick="toggleStatus('${safeId}', 'archive')"></i>
                </td>
                <td>
                    <div class="fw-bold fs-6">${bm.title}</div>
                    <a href="${bm.url}" target="_blank" class="text-muted small text-decoration-none">${bm.url}</a>
                </td>
                <td><span class="badge bg-secondary">${catName}</span></td>
                <td class="text-end text-nowrap">
                    <a href="bookmark.html?id=${safeId}" class="btn btn-sm btn-outline-info" title="View Details"><i class="bi bi-eye"></i></a>
                    <button class="btn btn-sm btn-outline-light mx-1" onclick="openEditModal('${safeId}')"><i class="bi bi-pencil"></i></button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteBookmark('${safeId}')"><i class="bi bi-trash"></i></button>
                </td>
            </tr>`;
    });
    dataContainer.innerHTML = html + `</tbody></table>`;
}

// --- Create & Edit Logic ---
function openCreateModal() {
    document.getElementById('bookmarkModalTitle').textContent = 'New Bookmark';
    document.getElementById('bookmark-form').reset();
    document.getElementById('bookmark-id').value = ''; // Clear ID for creation
    document.getElementById('bookmark-isFavorite').value = 'false';
    document.getElementById('bookmark-isArchived').value = 'false';
    bookmarkModalInstance.show();
}

window.openEditModal = function(id) {
    console.log("Attempting to edit ID:", id); // Debugging
    const bookmark = currentBookmarks.find(b => String(b.id || b.Id || b.bookmarkId) === String(id));
    
    if (!bookmark) {
        showAlert("Error: Could not find bookmark data in memory.", "danger");
        console.error("Available bookmarks:", currentBookmarks);
        return;
    }

    document.getElementById('bookmarkModalTitle').textContent = 'Edit Bookmark';
    // Using safeId for form population
    document.getElementById('bookmark-id').value = bookmark.id || bookmark.Id || bookmark.bookmarkId;
    document.getElementById('bookmark-title').value = bookmark.title;
    document.getElementById('bookmark-url').value = bookmark.url;
    
    // Robust category ID selection
    let catId = bookmark.categoryId || bookmark.CategoryId;
    if (catId) document.getElementById('bookmark-category').value = catId;

    document.getElementById('bookmark-isFavorite').value = bookmark.isFavorite;
    document.getElementById('bookmark-isArchived').value = bookmark.isArchived;

    bookmarkModalInstance.show();
};

async function saveBookmark() {
    const id = document.getElementById('bookmark-id').value;
    const title = document.getElementById('bookmark-title').value.trim();
    const url = document.getElementById('bookmark-url').value.trim();
    const categoryId = document.getElementById('bookmark-category').value;
    
    // Hidden fields for preserving state during edit
    const isFavorite = document.getElementById('bookmark-isFavorite').value === 'true';
    const isArchived = document.getElementById('bookmark-isArchived').value === 'true';

    if (!title || !url || !categoryId) {
        showAlert('Please fill in all required fields.', 'warning');
        return;
    }

    const payload = {
        title,
        url,
        categoryId: parseInt(categoryId),
        isFavorite,
        isArchived
    };

    if (id) payload.id = id;

    const method = id ? 'PUT' : 'POST';
    const apiUrl = id ? `${SINGLE_BOOKMARK_API}/${id}` : SINGLE_BOOKMARK_API;

    try {
        const response = await fetch(apiUrl, {
            method: method,
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            throw new Error(err.message || 'Failed to save bookmark');
        }

        bookmarkModalInstance.hide();
        showAlert(`Bookmark ${id ? 'updated' : 'created'} successfully!`);
        loadBookmarks();
    } catch (error) {
        showAlert(error.message, 'danger');
    }
}

// --- Quick Toggles ---
window.toggleStatus = async function(id, fieldToToggle) {
    console.log(`Toggling ${fieldToToggle} for ID:`, id); // Debugging
    const bookmark = currentBookmarks.find(b => String(b.id || b.Id || b.bookmarkId) === String(id));
    if (!bookmark) {
        console.error("Bookmark not found in memory for ID:", id);
        return;
    }

    // Toggle the specific boolean field
    const newIsFavorite = fieldToToggle === 'favorite' ? !bookmark.isFavorite : bookmark.isFavorite;
    const newIsArchived = fieldToToggle === 'archive' ? !bookmark.isArchived : bookmark.isArchived;

    const payload = {
        id: bookmark.id || bookmark.Id || bookmark.bookmarkId,
        title: bookmark.title,
        url: bookmark.url,
        categoryId: bookmark.categoryId || bookmark.CategoryId,
        isFavorite: newIsFavorite,
        isArchived: newIsArchived
    };

    try {
        const response = await fetch(`${SINGLE_BOOKMARK_API}/${id}`, {
            method: 'PUT', 
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` },
            body: JSON.stringify(payload) 
        });

        if (!response.ok) throw new Error(`Failed to update status`);
        
        // Optimistic UI update or reload
        loadBookmarks(); 
    } catch (error) {
        showAlert(error.message, 'danger');
    }
};


// --- Delete Logic ---
window.deleteBookmark = async function(id) {
    if (!confirm('Are you sure you want to delete this bookmark?')) return;
    try {
        const response = await fetch(`${SINGLE_BOOKMARK_API}/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });
        if (response.status === 204) {
            showAlert('Bookmark deleted successfully!');
            loadBookmarks();
        } else if (!response.ok) {
            throw new Error('Failed to delete bookmark');
        }
    } catch (error) {
        showAlert(error.message, 'danger');
    }
};