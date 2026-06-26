function togglePassword() {
    const input = document.getElementById('passwordInput');
    if (input) {
        input.type = input.type === 'password' ? 'text' : 'password';
    }
}