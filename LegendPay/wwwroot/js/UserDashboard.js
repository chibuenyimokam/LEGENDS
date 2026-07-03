// ── SIDEBAR COLLAPSE ──
const sidebar = document.getElementById('sidebar');
const mainContent = document.getElementById('main-content');
const toggleBtn = document.getElementById('sidebar-toggle');

toggleBtn.addEventListener('click', () => {
    sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('sidebar-collapsed');
});

// ── COPY WALLET ID ──
function copyWalletId() {
    const el = document.getElementById('wallet-id');
    const full = el.dataset.full || el.textContent.trim();
    navigator.clipboard.writeText(full).then(() => {
        const btn = document.querySelector('.copy-btn');
        btn.innerHTML = `<svg width="14" height="14" fill="none" stroke="#22C55E" stroke-width="2.5" viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12"/></svg>`;
        setTimeout(() => {
            btn.innerHTML = `<svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 01-2-2V4a2 2 0 012-2h9a2 2 0 012 2v1"/></svg>`;
        }, 2000);
    });
}