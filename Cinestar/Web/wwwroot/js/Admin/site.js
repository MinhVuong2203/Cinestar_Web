// Admin JavaScript Functions

// Sidebar Toggle
document.addEventListener('DOMContentLoaded', function () {
    const sidebarCollapse = document.getElementById('sidebarCollapse');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (sidebarCollapse) {
        sidebarCollapse.addEventListener('click', function () {
            sidebar.classList.toggle('active');
            content.classList.toggle('active');
        });
    }

    // Auto hide alerts after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(function (alert) {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

// Confirm Delete Function
function confirmDelete(message) {
    return confirm(message || 'Bạn có chắc chắn muốn xóa?');
}

// Image Preview Function
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            const preview = document.getElementById(previewId);
            if (preview) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            }
        };

        reader.readAsDataURL(input.files[0]);
    }
}

// Format Currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format Date
function formatDate(date) {
    return new Intl.DateTimeFormat('vi-VN').format(new Date(date));
}

// Datatable initialization (if using DataTables)
function initDataTable(tableId, options = {}) {
    const defaultOptions = {
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/vi.json'
        },
        pageLength: 2,
        responsive: true,
        ...options
    };

    return $(tableId).DataTable(defaultOptions);
}

// Form Validation Helper
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    }
}

// AJAX Helper
async function ajaxRequest(url, method = 'GET', data = null) {
    try {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
            }
        };

        if (data) {
            options.body = JSON.stringify(data);
        }

        const response = await fetch(url, options);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('AJAX Error:', error);
        throw error;
    }
}

// Show Loading Spinner
function showLoading(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = '<div class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>';
    }
}

// Toast Notification
function showToast(message, type = 'success') {
    const toastHTML = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    const toastContainer = document.querySelector('.toast-container');
    if (toastContainer) {
        toastContainer.insertAdjacentHTML('beforeend', toastHTML);
        const toastElement = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastElement);
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    }
}

// Export to Excel (using SheetJS library if available)
function exportToExcel(tableId, filename = 'export.xlsx') {
    const table = document.getElementById(tableId);
    if (table && typeof XLSX !== 'undefined') {
        const wb = XLSX.utils.table_to_book(table);
        XLSX.writeFile(wb, filename);
    } else {
        alert('Không thể xuất file Excel. Vui lòng kiểm tra thư viện SheetJS.');
    }
}

// Print Table
function printTable(tableId) {
    const table = document.getElementById(tableId);
    if (table) {
        const printWindow = window.open('', '', 'height=600,width=800');
        printWindow.document.write('<html><head><title>In bảng</title>');
        printWindow.document.write('<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">');
        printWindow.document.write('</head><body>');
        printWindow.document.write(table.outerHTML);
        printWindow.document.write('</body></html>');
        printWindow.document.close();
        printWindow.print();
    }
}

// Debounce Function for Search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'flex';
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'none';
}

document.addEventListener('DOMContentLoaded', function () {
    // Xử lý form submit
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            // Kiểm tra HTML5 validation
            if (!form.checkValidity()) {
                return; // Không show loading nếu form không hợp lệ
            }

            // Kiểm tra jQuery validation (nếu có sử dụng)
            if (typeof jQuery !== 'undefined' && jQuery(form).data('validator')) {
                const validator = jQuery(form).validate();
                if (!validator.form()) {
                    return; // Không show loading nếu validation error
                }
            }

            // Show loading nếu tất cả validation pass
            showLoading();

            // Tự động ẩn loading sau khi page load (trường hợp validation error từ server)
            setTimeout(function () {
                // Check nếu có validation errors từ server
                if (document.querySelector('.field-validation-error, .validation-summary-errors')) {
                    hideLoading();
                }
            }, 100);
        });
    });

    // Xử lý loading links
    document.querySelectorAll('.loading-link').forEach(function (link) {
        link.addEventListener('click', function () {
            const href = link.getAttribute('href');
            // Không show loading cho link # hoặc _blank
            if (href && href !== '#' && link.target !== '_blank') {
                showLoading();
            }
        });
    });
});

// Ẩn loading khi trang load xong
window.addEventListener('load', function () {
    hideLoading();
});

// Ẩn loading khi có validation error từ server (sau page load)
window.addEventListener('pageshow', function () {
    if (document.querySelector('.field-validation-error, .validation-summary-errors')) {
        hideLoading();
    }
});

