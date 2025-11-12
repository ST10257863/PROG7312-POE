// Maximum file size allowed for uploads (5MB)
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

// DOM elements for file upload and display
const dropzone = document.getElementById('dropzone');
const fileInput = document.getElementById('files');
const fileList = document.getElementById('file-list');

// Open file dialog when dropzone is clicked
dropzone.addEventListener('click', () => fileInput.click());

// Prevent default drag behaviors on dropzone
['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    dropzone.addEventListener(eventName, preventDefaults, false);
});

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

// Handle files dropped onto the dropzone
dropzone.addEventListener('drop', handleDrop, false);

// Handle files selected via file input
fileInput.addEventListener('change', handleFileSelect, false);

// Process files dropped onto the dropzone
function handleDrop(e) {
    let dt = e.dataTransfer;
    let files = dt.files;
    handleFiles(files);
}

// Process files selected via file input
function handleFileSelect(e) {
    let files = e.target.files;
    // Clear the display and input for new selection
    fileList.innerHTML = '';
    let dataTransfer = new DataTransfer();
    Array.from(files).forEach(file => {
        if (file.size > MAX_FILE_SIZE) {
            alert(`File "${file.name}" exceeds the 5MB size limit and will not be added.`);
            return;
        }
        dataTransfer.items.add(file);
        let fileSize = (file.size / 1024 / 1024).toFixed(2);
        let fileItem = document.createElement('div');
        fileItem.className = 'd-flex justify-content-between align-items-center mb-1';
        fileItem.innerHTML = `
            <span><i class="bi bi-file-earmark"></i> ${file.name} (${fileSize} MB)</span>
            <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile('${file.name}')">
                <i class="bi bi-x"></i>
            </button>
        `;
        fileList.appendChild(fileItem);
    });
    fileInput.files = dataTransfer.files;
}

// Add files to the file input and display list, enforcing size and duplicates
function handleFiles(files) {
    let dataTransfer = new DataTransfer();
    // Add existing files already in the input
    Array.from(fileInput.files).forEach(f => dataTransfer.items.add(f));
    // Add new files from drop or selection
    Array.from(files).forEach(file => {
        if (!fileAlreadyAdded(file)) {
            if (file.size > MAX_FILE_SIZE) {
                alert(`File "${file.name}" exceeds the 5MB size limit and will not be added.`);
                return;
            }
            dataTransfer.items.add(file);
            let fileSize = (file.size / 1024 / 1024).toFixed(2); // Convert to MB
            let fileItem = document.createElement('div');
            fileItem.className = 'd-flex justify-content-between align-items-center mb-1';
            fileItem.innerHTML = `
                <span><i class="bi bi-file-earmark"></i> ${file.name} (${fileSize} MB)</span>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile('${file.name}')">
                    <i class="bi bi-x"></i>
                </button>
            `;
            fileList.appendChild(fileItem);
        }
    });
    fileInput.files = dataTransfer.files;
}

// Check if a file is already in the display list
function fileAlreadyAdded(file) {
    return Array.from(fileList.children).some(item => item.textContent.includes(file.name));
}

// Remove a file from the file input and display list
window.removeFile = function (fileName) {
    let newFileList = Array.from(fileInput.files).filter(file => file.name !== fileName);
    updateFileList(newFileList);

    // Remove the file item from the display
    let fileItems = fileList.children;
    for (let i = 0; i < fileItems.length; i++) {
        if (fileItems[i].textContent.includes(fileName)) {
            fileItems[i].remove();
            break;
        }
    }
}

// Update the file input with a new list of files
function updateFileList(newFileList) {
    let dataTransfer = new DataTransfer();
    newFileList.forEach(file => {
        dataTransfer.items.add(file);
    });
    fileInput.files = dataTransfer.files;
}

// DOM elements for notification options and fields
const emailCheck = document.getElementById('emailCheck');
const phoneCheck = document.getElementById('phoneCheck');
const emailField = document.getElementById('emailField');
const phoneField = document.getElementById('phoneField');
const emailInput = document.getElementById('email');
const phoneInput = document.getElementById('phoneNumber');
const form = document.querySelector('form');

// Show/hide email field and set required attribute based on checkbox
emailCheck.addEventListener('change', function () {
    emailField.style.display = this.checked ? 'block' : 'none';
    emailInput.required = this.checked;
});

// Show/hide phone field and set required attribute based on checkbox
phoneCheck.addEventListener('change', function () {
    phoneField.style.display = this.checked ? 'block' : 'none';
    phoneInput.required = this.checked;
});

// Validate notification fields before form submission
form.addEventListener('submit', function (e) {
    if (emailCheck.checked && emailInput.value.trim() === '') {
        e.preventDefault();
        alert('Please enter your email address for notifications.');
    }
    if (phoneCheck.checked && phoneInput.value.trim() === '') {
        e.preventDefault();
        alert('Please enter your phone number for notifications.');
    }
});