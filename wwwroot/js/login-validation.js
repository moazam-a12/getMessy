$(document).ready(function () {
    function hideError(field) {
        $('#' + field + 'Error').addClass('hidden');
        $('#' + field).removeClass('border-red-500');
    }

    function showError(field, message) {
        $('#' + field + 'Error').text(message).removeClass('hidden');
        $('#' + field).addClass('border-red-500');
    }

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
        else if (value.length < 6) showError('password', 'Password must be at least 6 characters');
        else hideError('password');
    });

    $('#email, #password').on('focus', function () {
        hideError($(this).attr('id'));
    });

    $('#loginForm').on('submit', function (e) {
        var isValid = true;

        var email = $('#email').val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email === '') { showError('email', 'Email is required'); isValid = false; }
        else if (!emailRegex.test(email)) { showError('email', 'Please enter a valid email address'); isValid = false; }

        var password = $('#password').val();
        if (password === '') { showError('password', 'Password is required'); isValid = false; }
        else if (password.length < 6) { showError('password', 'Password must be at least 6 characters'); isValid = false; }

        if (!isValid) {
            e.preventDefault();
            $('.bg-red-100').addClass('animate-shake');
            setTimeout(function () { $('.bg-red-100').removeClass('animate-shake'); }, 500);
        }
    });
});
