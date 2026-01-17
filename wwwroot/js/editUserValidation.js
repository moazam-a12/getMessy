$(document).ready(function () {
    function hideError(field) {
        $('#' + field + 'Error').addClass('hidden');
        $('#' + field).removeClass('border-red-500');
    }

    function showError(field, message) {
        $('#' + field + 'Error').text(message).removeClass('hidden');
        $('#' + field).addClass('border-red-500');
    }

    // Full Name validation
    $('#fullName').on('blur', function () {
        var value = $(this).val().trim();
        if (value === '') {
            showError('fullName', 'Full name is required');
        } else if (value.length < 3) {
            showError('fullName', 'Full name must be at least 3 characters');
        } else {
            hideError('fullName');
        }
    });

    // Email validation
    $('#email').on('blur', function () {
        var value = $(this).val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (value === '') {
            showError('email', 'Email is required');
        } else if (!emailRegex.test(value)) {
            showError('email', 'Please enter a valid email address');
        } else {
            hideError('email');
        }
    });

    // New Password validation (only if filled)
    $('#newPassword').on('blur', function () {
        var value = $(this).val();
        if (value !== '' && value.length < 6) {
            showError('newPassword', 'Password must be at least 6 characters');
        } else {
            hideError('newPassword');
        }
    });

    // Clear errors on focus
    $('#fullName, #email, #newPassword').on('focus', function () {
        var fieldId = $(this).attr('id');
        hideError(fieldId);
    });

    // Form submission validation
    $('#editUserForm').on('submit', function (e) {
        var isValid = true;

        // Validate Full Name
        var fullName = $('#fullName').val().trim();
        if (fullName === '') {
            showError('fullName', 'Full name is required');
            isValid = false;
        } else if (fullName.length < 3) {
            showError('fullName', 'Full name must be at least 3 characters');
            isValid = false;
        } else {
            hideError('fullName');
        }

        // Validate Email
        var email = $('#email').val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email === '') {
            showError('email', 'Email is required');
            isValid = false;
        } else if (!emailRegex.test(email)) {
            showError('email', 'Please enter a valid email address');
            isValid = false;
        } else {
            hideError('email');
        }

        // Validate New Password (only if filled)
        var newPassword = $('#newPassword').val();
        if (newPassword !== '' && newPassword.length < 6) {
            showError('newPassword', 'Password must be at least 6 characters');
            isValid = false;
        } else {
            hideError('newPassword');
        }

        if (!isValid) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: $('.border-red-500').first().offset().top - 100
            }, 300);
        }
    });
});
