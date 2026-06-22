function toggleWallet() {
    const checkmark = document.getElementById('checkmark');
    const checkbox = document.getElementById('walletCheckbox');

    if (checkmark.style.display === 'none') {
        checkmark.style.display = 'block';
        checkbox.style.background = 'var(--blue)';
    } else {
        checkmark.style.display = 'none';
        checkbox.style.background = 'transparent';
    }
}