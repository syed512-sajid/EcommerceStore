// signup.js

document.addEventListener('DOMContentLoaded', function () {
    const password = document.getElementById('password');
    const confirmPassword = document.getElementById('confirmPassword');

    if (confirmPassword) {
        confirmPassword.addEventListener('input', function () {
            if (password.value !== confirmPassword.value) {
                confirmPassword.setCustomValidity('Passwords do not match');
            } else {
                confirmPassword.setCustomValidity('');
            }
        });
    }
});
