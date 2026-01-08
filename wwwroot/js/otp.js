// OTP Verification Page JavaScript

(function () {
    'use strict';

    // DOM Elements
    const inputs = document.querySelectorAll('.otp-input');
    const form = document.getElementById('otpForm');
    const otpValue = document.getElementById('otpValue');
    const verifyBtn = document.getElementById('verifyBtn');
    const resendLink = document.getElementById('resendLink');
    const resendTimerSpan = document.getElementById('resendTimer');
    const timerElement = document.getElementById('timeLeft');
    const timerContainer = document.getElementById('timer');

    // Timer Variables
    let otpExpiryTime = 300; // 5 minutes in seconds
    let resendCooldown = 60; // 60 seconds cooldown
    let expiryInterval;
    let resendInterval;

    // ===============================
    // OTP INPUT HANDLING
    // ===============================

    /**
     * Update hidden OTP value field
     */
    function updateOtpValue() {
        const otp = Array.from(inputs).map(input => input.value).join('');
        otpValue.value = otp;
        return otp;
    }

    /**
     * Handle input event
     */
    function handleInput(e, index) {
        const value = e.target.value;

        // Only allow numbers
        if (!/^[0-9]$/.test(value)) {
            e.target.value = '';
            return;
        }

        // Move to next input
        if (value && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }

        // Update hidden field
        updateOtpValue();

        // Auto-submit if all 6 digits entered
        if (updateOtpValue().length === 6) {
            setTimeout(() => {
                form.submit();
            }, 200);
        }
    }

    /**
     * Handle keydown event
     */
    function handleKeydown(e, index) {
        // Backspace: clear current and move to previous
        if (e.key === 'Backspace') {
            e.preventDefault();
            inputs[index].value = '';
            if (index > 0) {
                inputs[index - 1].focus();
            }
            updateOtpValue();
        }

        // Left arrow
        if (e.key === 'ArrowLeft' && index > 0) {
            inputs[index - 1].focus();
        }

        // Right arrow
        if (e.key === 'ArrowRight' && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }
    }

    /**
     * Handle paste event
     */
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

            // Focus last filled input or submit
            const lastIndex = Math.min(digits.length - 1, 5);
            inputs[lastIndex].focus();

            // Auto-submit if 6 digits pasted
            if (digits.length >= 6) {
                setTimeout(() => {
                    form.submit();
                }, 200);
            }
        }
    }

    /**
     * Initialize input handlers
     */
    function initializeInputs() {
        inputs.forEach((input, index) => {
            // Input event
            input.addEventListener('input', (e) => handleInput(e, index));

            // Keydown event
            input.addEventListener('keydown', (e) => handleKeydown(e, index));

            // Paste event
            input.addEventListener('paste', handlePaste);

            // Focus event - select all
            input.addEventListener('focus', (e) => {
                e.target.select();
            });
        });
    }

    // ===============================
    // FORM SUBMISSION
    // ===============================

    function handleFormSubmit(e) {
        updateOtpValue();

        if (otpValue.value.length !== 6) {
            e.preventDefault();
            alert('Please enter all 6 digits');
            inputs[0].focus();
            return false;
        }

        // Disable button to prevent double submission
        verifyBtn.disabled = true;
        verifyBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Verifying...';
    }

    // ===============================
    // TIMER FUNCTIONS
    // ===============================

    /**
     * Format seconds to MM:SS
     */
    function formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }

    /**
     * Start OTP expiry countdown
     */
    function startExpiryTimer() {
        expiryInterval = setInterval(() => {
            otpExpiryTime--;
            timerElement.textContent = formatTime(otpExpiryTime);

            // Warning at 1 minute
            if (otpExpiryTime === 60) {
                timerContainer.style.color = '#ff6b6b';
            }

            // Expired
            if (otpExpiryTime <= 0) {
                clearInterval(expiryInterval);
                timerContainer.classList.add('expired');
                timerElement.textContent = 'EXPIRED';
                verifyBtn.disabled = true;
                verifyBtn.innerHTML = '<i class="fas fa-exclamation-triangle me-2"></i>OTP Expired';

                // Disable all inputs
                inputs.forEach(input => input.disabled = true);
            }
        }, 1000);
    }

    /**
     * Start resend countdown
     */
    function startResendTimer() {
        resendInterval = setInterval(() => {
            resendCooldown--;
            resendTimerSpan.textContent = resendCooldown;

            if (resendCooldown <= 0) {
                clearInterval(resendInterval);
                resendLink.classList.remove('disabled');
                resendLink.innerHTML = '<i class="fas fa-redo me-1"></i> Resend OTP';
            }
        }, 1000);
    }

    // ===============================
    // RESEND OTP
    // ===============================

    function handleResendClick(e) {
        e.preventDefault();

        if (!resendLink.classList.contains('disabled')) {
            // Show loading state
            resendLink.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Sending...';

            // Redirect to resend action
            if (window.resendOtpUrl) {
                window.location.href = window.resendOtpUrl;
            } else {
                console.error('Resend URL not configured');
                resendLink.innerHTML = '<i class="fas fa-exclamation-circle me-1"></i> Error';
            }
        }
    }

    // ===============================
    // INITIALIZATION
    // ===============================

    function init() {
        // Initialize input handlers
        initializeInputs();

        // Form submission
        if (form) {
            form.addEventListener('submit', handleFormSubmit);
        }

        // Resend link
        if (resendLink) {
            resendLink.addEventListener('click', handleResendClick);
        }

        // Start timers
        startExpiryTimer();
        startResendTimer();

        // Focus first input
        if (inputs.length > 0) {
            inputs[0].focus();
        }

        // Auto-dismiss alerts after 5 seconds
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000);
        });
    }

    // ===============================
    // PAGE LOAD
    // ===============================

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // ===============================
    // CLEANUP ON PAGE UNLOAD
    // ===============================

    window.addEventListener('beforeunload', () => {
        if (expiryInterval) clearInterval(expiryInterval);
        if (resendInterval) clearInterval(resendInterval);
    });

})();