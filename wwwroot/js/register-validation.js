$(document).ready(function () {
    function hideError(field) {
        $('#' + field + 'Error').addClass('hidden');
        $('#' + field).removeClass('border-red-500');
    }

    function showError(field, message) {
        $('#' + field + 'Error').text(message).removeClass('hidden');
        $('#' + field).addClass('border-red-500');
    }

    $('#fullName').on('blur', function () {
        var value = $(this).val().trim();
        if (value === '') showError('fullName', 'Full name is required');
        else hideError('fullName');
    });

    $('#email').on('blur', function () {
        var value = $(this).val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (value === '') showError('email', 'Email is required');
        else if (!emailRegex.test(value)) showError('email', 'Please enter a valid email address');
        else hideError('email');
    });

    $('#password').on('blur', function () {
        var value = $(this).val();
        if (value === '') showError('password', 'Password is required');
        else if (value.length < 6) showError('password', 'Password must be at least 6 characters long');
        else hideError('password');
    });

    $('#confirmPassword').on('blur', function () {
        var password = $('#password').val();
        var confirmPassword = $(this).val();
        if (confirmPassword === '') showError('confirmPassword', 'Please confirm your password');
        else if (password !== confirmPassword) showError('confirmPassword', 'Passwords do not match');
        else hideError('confirmPassword');
    });

    $('#registerForm').on('submit', function (e) {
        var isValid = true;
        var fullName = $('#fullName').val().trim();
        if (fullName === '') { showError('fullName', 'Full name is required'); isValid = false; }

        var email = $('#email').val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email === '') { showError('email', 'Email is required'); isValid = false; }
        else if (!emailRegex.test(email)) { showError('email', 'Please enter a valid email address'); isValid = false; }

        var password = $('#password').val();
        if (password === '') { showError('password', 'Password is required'); isValid = false; }
        else if (password.length < 6) { showError('password', 'Password must be at least 6 characters long'); isValid = false; }

        var confirmPassword = $('#confirmPassword').val();
        if (confirmPassword === '') { showError('confirmPassword', 'Please confirm your password'); isValid = false; }
        else if (password !== confirmPassword) { showError('confirmPassword', 'Passwords do not match'); isValid = false; }

        if (!isValid) e.preventDefault();
    });
});
