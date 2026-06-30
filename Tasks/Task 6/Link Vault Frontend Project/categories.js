const API_URL = 'http://linkvaultapi.runasp.net/api/Categories'; 
const dataContainer = document.getElementById('data-container');
let categoryModalInstance; 

// Initialize Bootstrap Modal on page load
document.addEventListener('DOMContentLoaded', () => {
    categoryModalInstance = new bootstrap.Modal(document.getElementById('categoryModal'));
    loadCategories();
});

// --- Utility: Show Alerts ---
function showAlert(message, type = 'success') {
    const alertContainer = document.getElementById('alert-container');
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show shadow" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    alertContainer.innerHTML = alertHtml;
    // Auto-dismiss after 4 seconds
    setTimeout(() => {
        const alertNode = alertContainer.querySelector('.alert');
        if (alertNode) {
            const bsAlert = new bootstrap.Alert(alertNode);
            bsAlert.close();
        }
    }, 4000);
}

// --- Fetch & Render Categories ---
async function loadCategories() {
    dataContainer.innerHTML = '<p class="text-center text-muted my-4">Loading categories...</p>';
    
    try {
        const response = await fetch(API_URL, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });

        if (response.status === 401) {
            localStorage.removeItem('jwt_token');
            window.location.href = 'login.html';
            return;
        }
        
        if (!response.ok) throw new Error('Failed to fetch categories');

        const categories = await response.json();
        renderTable(categories);

    } catch (error) {
        dataContainer.innerHTML = `<p class="text-center text-danger my-4">${error.message}</p>`;
    }
}

function renderTable(categories) {
    if (!categories || categories.length === 0) {
        dataContainer.innerHTML = `
            <div class="text-center py-5">
                <i class="bi bi-folder2-open display-4 text-muted"></i>
                <p class="mt-3 text-muted">No categories found. Create one to get started!</p>
            </div>`;
        return;
    }

    let html = `
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th>Category Name</th>
                    <th>Description</th>
                    <th class="text-end">Actions</th>
                </tr>
            </thead>
            <tbody>
    `;

    categories.forEach(cat => {
        // Updated cat.name to cat.categoryName based on Swagger
        html += `
            <tr>
                <td class="fw-bold">${cat.categoryName}</td>
                <td class="text-muted small">${cat.description || '—'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-secondary me-1" onclick="openEditModal('${cat.id}', '${cat.categoryName}', '${cat.description || ''}')">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteCategory('${cat.id}')">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    html += `</tbody></table>`;
    dataContainer.innerHTML = html;
}

// --- Create & Edit Logic ---
document.getElementById('create-new-btn').addEventListener('click', () => {
    document.getElementById('categoryModalTitle').textContent = 'New Category';
    document.getElementById('category-form').reset();
    document.getElementById('category-id').value = ''; 
    categoryModalInstance.show();
});

// Called inline from the table's Edit button
window.openEditModal = function(id, categoryName, description) {
    document.getElementById('categoryModalTitle').textContent = 'Edit Category';
    document.getElementById('category-id').value = id;
    document.getElementById('category-name').value = categoryName; // Maps to the HTML input
    document.getElementById('category-desc').value = description;
    categoryModalInstance.show();
};

document.getElementById('save-category-btn').addEventListener('click', async () => {
    const id = document.getElementById('category-id').value;
    const inputName = document.getElementById('category-name').value.trim();
    const description = document.getElementById('category-desc').value.trim();

    if (!inputName) {
        showAlert('Category name is required.', 'danger');
        return;
    }

    // Updated payload to match Swagger { "categoryName": "string", "description": "string" }
    const payload = { 
        categoryName: inputName, 
        description: description 
    };
    
    // PUT endpoints require all fields in the body [cite: 83]
    if (id) payload.id = id; 

    const method = id ? 'PUT' : 'POST';
    const endpoint = id ? `${API_URL}/${id}` : API_URL;

    try {
        const response = await fetch(endpoint, {
            method: method,
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errData = await response.json().catch(() => null);
            throw new Error(errData?.message || 'Failed to save category');
        }

        showAlert(id ? 'Category updated successfully!' : 'Category created successfully!');
        categoryModalInstance.hide();
        loadCategories(); // Refresh the table

    } catch (error) {
        showAlert(error.message, 'danger');
    }
});

// --- Delete Logic ---
window.deleteCategory = async function(id) {
    if (!confirm('Are you sure you want to delete this category?')) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
        });

        if (response.status === 204) {
            showAlert('Category deleted successfully!');
            loadCategories();
        } else if (!response.ok) {
            const errData = await response.json().catch(() => null);
            throw new Error(errData?.message || 'Cannot delete category. It may contain bookmarks or notes.');
        }

    } catch (error) {
        showAlert(error.message, 'danger');
    }
};