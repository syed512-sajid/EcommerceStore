// ========================================
// COMPLETE AUTH.JS - ALL SCRIPTS MERGED
// ========================================

// ===== DASHBOARD SCRIPTS =====
document.addEventListener("DOMContentLoaded", function () {
    console.log("Dashboard scripts loaded.");

    // Timeline animations
    const timelineItems = document.querySelectorAll('.timeline-item');
    timelineItems.forEach(item => {
        if (item.classList.contains('active')) {
            item.style.transform = 'translateX(0)';
            item.style.opacity = 1;
        } else {
            item.style.transform = 'translateX(-20px)';
            item.style.opacity = 0.5;
        }
    });
});

// ===== SIGNIN PAGE =====
document.addEventListener('DOMContentLoaded', function () {
    const emailInput = document.querySelector('input[name="email"]');
    if (emailInput) emailInput.focus();
});

// ===== SIGNUP PAGE - PASSWORD MATCH VALIDATION =====
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

// ===== OTP VERIFICATION LOGIC =====
(function () {
    'use strict';

    const inputs = document.querySelectorAll('.otp-input');
    const form = document.getElementById('otpForm');
    const otpValue = document.getElementById('otpValue');
    const verifyBtn = document.getElementById('verifyBtn');
    const resendLink = document.getElementById('resendLink');
    const resendTimerSpan = document.getElementById('resendTimer');
    const timerElement = document.getElementById('timeLeft');
    const timerContainer = document.getElementById('timer');

    if (!inputs.length) return; // Exit if not on OTP page

    let otpExpiryTime = 300; // 5 minutes
    let resendCooldown = 60; // 60 seconds
    let expiryInterval;
    let resendInterval;

    function updateOtpValue() {
        const otp = Array.from(inputs).map(input => input.value).join('');
        if (otpValue) otpValue.value = otp;
        return otp;
    }

    function handleInput(e, index) {
        const value = e.target.value;

        if (!/^[0-9]$/.test(value)) {
            e.target.value = '';
            return;
        }

        if (value && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }

        updateOtpValue();

        if (updateOtpValue().length === 6) {
            setTimeout(() => {
                if (form) form.submit();
            }, 200);
        }
    }

    function handleKeydown(e, index) {
        if (e.key === 'Backspace') {
            e.preventDefault();
            inputs[index].value = '';
            if (index > 0) {
                inputs[index - 1].focus();
            }
            updateOtpValue();
        }

        if (e.key === 'ArrowLeft' && index > 0) {
            inputs[index - 1].focus();
        }

        if (e.key === 'ArrowRight' && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }
    }

    function handlePaste(e) {
        e.preventDefault();
        const pasteData = e.clipboardData.getData('text');
        const digits = pasteData.match(/\d/g);

        if (digits) {
            digits.slice(0, 6).forEach((digit, i) => {
                if (inputs[i]) {
                    inputs[i].value = digit;
                }
            });
            updateOtpValue();

            const lastIndex = Math.min(digits.length - 1, 5);
            inputs[lastIndex].focus();

            if (digits.length >= 6) {
                setTimeout(() => {
                    if (form) form.submit();
                }, 200);
            }
        }
    }

    function initializeInputs() {
        inputs.forEach((input, index) => {
            input.addEventListener('input', (e) => handleInput(e, index));
            input.addEventListener('keydown', (e) => handleKeydown(e, index));
            input.addEventListener('paste', handlePaste);
            input.addEventListener('focus', (e) => {
                e.target.select();
            });
        });
    }

    function handleFormSubmit(e) {
        updateOtpValue();

        if (otpValue && otpValue.value.length !== 6) {
            e.preventDefault();
            alert('Please enter all 6 digits');
            inputs[0].focus();
            return false;
        }

        if (verifyBtn) {
            verifyBtn.disabled = true;
            verifyBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Verifying...';
        }
    }

    function formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }

    function startExpiryTimer() {
        if (!timerElement) return;

        expiryInterval = setInterval(() => {
            otpExpiryTime--;
            timerElement.textContent = formatTime(otpExpiryTime);

            if (otpExpiryTime === 60 && timerContainer) {
                timerContainer.style.color = '#ff6b6b';
            }

            if (otpExpiryTime <= 0) {
                clearInterval(expiryInterval);
                if (timerContainer) timerContainer.classList.add('expired');
                if (timerElement) timerElement.textContent = 'EXPIRED';
                if (verifyBtn) {
                    verifyBtn.disabled = true;
                    verifyBtn.innerHTML = '<i class="fas fa-exclamation-triangle me-2"></i>OTP Expired';
                }

                inputs.forEach(input => input.disabled = true);
            }
        }, 1000);
    }

    function startResendTimer() {
        if (!resendTimerSpan) return;

        resendInterval = setInterval(() => {
            resendCooldown--;
            resendTimerSpan.textContent = resendCooldown;

            if (resendCooldown <= 0) {
                clearInterval(resendInterval);
                if (resendLink) {
                    resendLink.classList.remove('disabled');
                    resendLink.innerHTML = '<i class="fas fa-redo me-1"></i> Resend OTP';
                }
            }
        }, 1000);
    }

    function handleResendClick(e) {
        e.preventDefault();

        if (resendLink && !resendLink.classList.contains('disabled')) {
            resendLink.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Sending...';

            if (window.resendOtpUrl) {
                window.location.href = window.resendOtpUrl;
            } else {
                console.error('Resend URL not configured');
                resendLink.innerHTML = '<i class="fas fa-exclamation-circle me-1"></i> Error';
            }
        }
    }

    function init() {
        initializeInputs();

        if (form) {
            form.addEventListener('submit', handleFormSubmit);
        }

        if (resendLink) {
            resendLink.addEventListener('click', handleResendClick);
        }

        startExpiryTimer();
        startResendTimer();

        if (inputs.length > 0) {
            inputs[0].focus();
        }

        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            setTimeout(() => {
                if (typeof bootstrap !== 'undefined') {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                }
            }, 5000);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.addEventListener('beforeunload', () => {
        if (expiryInterval) clearInterval(expiryInterval);
        if (resendInterval) clearInterval(resendInterval);
    });

})();