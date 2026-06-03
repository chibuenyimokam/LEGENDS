function togglePassword() {
    const input = document.getElementById('passwordInput');
    input.type = input.type === 'password' ? 'text' : 'password';
}

function handleSubmit(e) {
    const otpSection = document.getElementById('otpSection');
    const isOtpVisible = otpSection.classList.contains('show');

    if (isOtpVisible) {
        const form = document.getElementById('loginForm');
        form.action = verifyOtpUrl;
    }

    const btn = document.getElementById('submitBtn');
    btn.classList.add('loading');
    btn.disabled = true;
}

if (otpVisible) {
    document.getElementById('otpSection').classList.add('show');
}