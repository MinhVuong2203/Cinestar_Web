const imageFile = document.getElementById('ImageFile');
const preview = document.getElementById('PreviewImage');
const previewContainer = document.getElementById('ImagePreview');

imageFile.addEventListener('change', function (event) {
    const file = event.target.files[0];

    if (file) {
        // Kiểm tra định dạng
        const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!validTypes.includes(file.type)) {
            alert('Chỉ chấp nhận file ảnh JPG, PNG, GIF.');
            imageFile.value = '';
            return;
        }

        // Hiển thị preview
        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            previewContainer.style.display = 'block';
        };
        reader.readAsDataURL(file);
    } else {
        preview.src = '#';
        previewContainer.style.display = 'none';
    }
});


const togglePassword = document.getElementById('togglePassword');
const passwordInput = document.getElementById('PasswordInput');

togglePassword.addEventListener('click', function () {
    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordInput.setAttribute('type', type);

    // Đổi icon con mắt
    this.querySelector('i').classList.toggle('fa-eye');
    this.querySelector('i').classList.toggle('fa-eye-slash');
});
